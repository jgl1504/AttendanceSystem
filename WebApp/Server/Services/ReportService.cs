using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Server.Data;
using WebApp.Shared.Model;

public class ReportService
{
    private readonly DataContext _context;

    public ReportService(DataContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Per-employee overtime summary for the given date range.
    /// Work date is derived from ClockInTime.Date.
    /// </summary>
    public async Task<List<DriverOvertimeSummary>> GetDriverOvertimeSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        int? employeeId)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        // Base query for records in range
        var recordsQuery =
            from a in _context.AttendanceRecords
            let workDate = a.ClockInTime.Date
            where workDate >= startDate && workDate <= endDate
            select a;

        if (employeeId.HasValue && employeeId.Value > 0)
            recordsQuery = recordsQuery.Where(a => a.EmployeeId == employeeId.Value);

        var query =
            from a in recordsQuery
            group a by new { a.EmployeeId, a.Employee.Name } into g
            select new DriverOvertimeSummary
            {
                EmployeeId = g.Key.EmployeeId,
                EmployeeName = g.Key.Name,
                StartDate = startDate,
                EndDate = endDate,

                // Total normal hours = HoursWorked - overtime parts (null safe)
                NormalHours =
                    g.Sum(x => x.HoursWorked ?? 0)
                    - g.Sum(x => x.WeekdayOvertimeHours ?? 0)
                    - g.Sum(x => x.SundayPublicOvertimeHours ?? 0),

                WeekdayOvertimeHours =
                    g.Sum(x => x.WeekdayOvertimeHours ?? 0),

                SundayPublicOvertimeHours =
                    g.Sum(x => x.SundayPublicOvertimeHours ?? 0),

                ApprovedOvertimeHours =
                    g.Where(x => x.OvertimeStatus == OvertimeStatus.Approved)
                     .Sum(x => (x.WeekdayOvertimeHours ?? 0)
                             + (x.SundayPublicOvertimeHours ?? 0)),

                UnapprovedOvertimeHours =
                    g.Where(x => x.OvertimeStatus != OvertimeStatus.Approved)
                     .Sum(x => (x.WeekdayOvertimeHours ?? 0)
                             + (x.SundayPublicOvertimeHours ?? 0))
            };

        return await query
            .OrderBy(x => x.EmployeeName)
            .ToListAsync();
    }

    /// <summary>
    /// Per-employee leave summary (per leave type) for the given date range.
    /// Uses LeaveRecord + LeaveBalance.
    /// </summary>
    public async Task<List<LeaveSummary>> GetLeaveSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        int? employeeId)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        // 1) Approved leave records that overlap the period
        var takenBaseQuery =
            from r in _context.LeaveRecords   // EF entity matching LeaveRecordDto
            where r.Status == LeaveStatus.Approved
                  // any overlap with the selected period
                  && r.StartDate.Date <= endDate
                  && r.EndDate.Date >= startDate
            select r;

        if (employeeId.HasValue && employeeId.Value > 0)
            takenBaseQuery = takenBaseQuery.Where(r => r.EmployeeId == employeeId.Value);

        var takenQuery =
            from r in takenBaseQuery
            group r by new
            {
                r.EmployeeId,
                EmployeeName = r.Employee.Name,
                r.LeaveTypeId,
                LeaveTypeName = r.LeaveType.Name
            }
            into g
            select new
            {
                g.Key.EmployeeId,
                g.Key.EmployeeName,
                g.Key.LeaveTypeId,
                g.Key.LeaveTypeName,
                Taken = g.Sum(x => x.DaysTaken)
            };

        var taken = await takenQuery.ToListAsync();

        if (!taken.Any())
        {
            return new List<LeaveSummary>();
        }

        // 2) Fetch balances for the employees + leave types in the result
        var employeeIds = taken.Select(x => x.EmployeeId).Distinct().ToList();
        var typeIds = taken.Select(x => x.LeaveTypeId).Distinct().ToList();

        var balancesQuery =
            from b in _context.LeaveBalances
            where employeeIds.Contains(b.EmployeeId)
                  && typeIds.Contains(b.LeaveTypeId)
            select new
            {
                b.EmployeeId,
                b.LeaveTypeId,
                // You can decide whether TotalEntitlement is OpeningBalance or CurrentBalance
                TotalEntitlement = b.OpeningBalance,
                Remaining = b.CurrentBalance
            };

        var balances = await balancesQuery.ToListAsync();

        // 3) Build per-employee LeaveSummary with a list of LeaveBalanceSummaryDto rows
        var summaries =
            taken
                .GroupBy(x => new { x.EmployeeId, x.EmployeeName })
                .Select(g =>
                {
                    var items = new List<LeaveBalanceSummaryDto>();

                    foreach (var t in g)
                    {
                        var bal = balances.FirstOrDefault(b =>
                            b.EmployeeId == t.EmployeeId &&
                            b.LeaveTypeId == t.LeaveTypeId);

                        items.Add(new LeaveBalanceSummaryDto
                        {
                            LeaveTypeName = t.LeaveTypeName,
                            TotalEntitlement = bal?.TotalEntitlement ?? 0,
                            Taken = t.Taken,
                            Remaining = bal?.Remaining ?? 0
                        });
                    }

                    return new LeaveSummary
                    {
                        EmployeeId = g.Key.EmployeeId,
                        EmployeeName = g.Key.EmployeeName,
                        StartDate = startDate,
                        EndDate = endDate,
                        LeaveTypes = items
                    };
                })
                .OrderBy(x => x.EmployeeName)
                .ToList();

        return summaries;
    }

    public async Task<List<SaturdayWorkSummary>> GetSaturdayWorkReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? employeeId)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        // Employees (with departments) – EF query
        var employeesQuery = _context.Employees
            .Include(e => e.Department)
            .Where(e => e.IsActive);

        if (employeeId.HasValue && employeeId.Value > 0)
            employeesQuery = employeesQuery.Where(e => e.Id == employeeId.Value);

        var employees = await employeesQuery.ToListAsync();

        // Time range in UTC for SQL query
        var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
        var endUtc = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Local).ToUniversalTime();

        // Attendance in range – EF query
        var attendanceAllQuery = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Where(a => a.ClockInTime >= startUtc && a.ClockInTime < endUtc);

        if (employeeId.HasValue && employeeId.Value > 0)
            attendanceAllQuery = attendanceAllQuery.Where(a => a.EmployeeId == employeeId.Value);

        var attendanceAll = await attendanceAllQuery.ToListAsync();

        // Now in memory: keep only Saturdays
        var saturdayAttendance = attendanceAll
            .Where(a => a.ClockInTime.ToLocalTime().DayOfWeek == DayOfWeek.Saturday)
            .ToList();

        var summaries = new List<SaturdayWorkSummary>();

        foreach (var emp in employees)
        {
            var dept = emp.Department;
            if (dept == null)
                continue;

            // 1) Count Saturdays in range the employee is expected to work
            var expected = 0;
            if (dept.WorksSaturday)
            {
                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    if (d.DayOfWeek == DayOfWeek.Saturday)
                        expected++;
                }
            }

            // 2) Saturdays actually worked
            var worked = saturdayAttendance
                .Where(a => a.EmployeeId == emp.Id)
                .Select(a => a.ClockInTime.ToLocalTime().Date)
                .Distinct()
                .Count();

            var missed = Math.Max(0, expected - worked);

            summaries.Add(new SaturdayWorkSummary
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                DepartmentName = dept.Name,
                ExpectedSaturdays = expected,
                WorkedSaturdays = worked,
                MissedSaturdays = missed
            });
        }

        return summaries;
    }
}
