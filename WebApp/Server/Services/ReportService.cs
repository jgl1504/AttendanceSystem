using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Server.Data;
using WebApp.Server.Services.Attendance;
using WebApp.Shared.Model;
using WebApp.Shared.Model.Payroll;

public class ReportService
{
    private readonly DataContext _context;
    private readonly AttendanceService _attendanceService;

    public ReportService(DataContext context, AttendanceService attendanceService)
    {
        _context = context;
        _attendanceService = attendanceService;
    }

    public async Task<List<PayrollTimeSummaryDto>> GetPayrollTimeSummaryAsync(
        DateTime startDateInclusive,
        DateTime endDateExclusive,
        int? companyId,
        int? departmentId,
        int? employeeId)
    {
        var attendance = await _attendanceService.GetByDateRangeAsync(
            fromLocalInclusive: startDateInclusive,
            toLocalExclusive: endDateExclusive,
            employeeId: employeeId,
            departmentId: departmentId,
            companyId: companyId);

        // 1) Collapse to one row per employee per day
        var dailyPerEmployee = attendance
            .GroupBy(a => new
            {
                a.EmployeeName,
                Day = a.ClockInTime.ToLocalTime().Date
            })
            .Select(g => new
            {
                g.Key.EmployeeName,
                g.Key.Day,

                // Daily normal hours = sum of NormalHours across that day
                Normal = g.Sum(x => x.NormalHours ?? 0),

                // Daily OT buckets and status are identical for all rows that day,
                // because GetByDateRangeAsync already sets them per day.
                WeekdayOT = g.First().WeekdayOvertimeHours ?? 0,
                SundayOT = g.First().SundayPublicOvertimeHours ?? 0,
                Status = g.First().OvertimeStatus,

                // Daily approved driver hours = sum of ApprovedDriverHours
                DriverApproved = g.Sum(x => x.ApprovedDriverHours ?? 0)
            })
            .ToList();

        // 2) Now aggregate per employee over the whole payroll period, using only Approved where required
        var grouped = dailyPerEmployee
            .GroupBy(x => x.EmployeeName)
            .Select(g => new PayrollTimeSummaryDto
            {
                EmployeeName = g.Key,
                StartDate = startDateInclusive.Date,
                EndDate = endDateExclusive.Date,

                NormalHours = g.Sum(x => x.Normal),

                OvertimeWeekdayApproved = g
                    .Where(x => x.Status == OvertimeStatus.Approved)
                    .Sum(x => x.WeekdayOT),

                OvertimeSundayApproved = g
                    .Where(x => x.Status == OvertimeStatus.Approved)
                    .Sum(x => x.SundayOT),

                DriverApproved = g.Sum(x => x.DriverApproved)
            })
            .OrderBy(r => r.EmployeeName)
            .ToList();

        return grouped;
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

        var takenBaseQuery =
            from r in _context.LeaveRecords
            where r.Status == LeaveStatus.Approved
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
                TotalEntitlement = b.OpeningBalance,
                Remaining = b.CurrentBalance
            };

        var balances = await balancesQuery.ToListAsync();

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

    /// <summary>
    /// Payroll leave matrix: one row per employee with balances per leave type
    /// as at end of previous month for the selected payroll month (year, month).
    /// Reuses GetLeaveSummaryAsync + LeaveBalances.
    /// </summary>
    public async Task<List<PayrollLeaveRowDto>> GetPayrollLeaveMatrixAsync(
        int year,
        int month,
        int? companyId,
        int? departmentId,
        int? employeeId)
    {
        // Previous calendar month range
        var selected = new DateTime(year, month, 1);
        var prev = selected.AddMonths(-1);
        var prevStart = new DateTime(prev.Year, prev.Month, 1);
        var prevEnd = prevStart.AddMonths(1).AddDays(-1);

        // Start from LeaveSummary (per employee, per type)
        var leaveSummaries = await GetLeaveSummaryAsync(prevStart, prevEnd, employeeId);

        if (!leaveSummaries.Any())
            return new List<PayrollLeaveRowDto>();

        // Apply company / department filters via Employees table
        if (companyId.HasValue || departmentId.HasValue)
        {
            var ids = leaveSummaries.Select(s => s.EmployeeId).Distinct().ToList();

            var employeesQuery = _context.Employees.AsQueryable();

            if (companyId.HasValue && companyId.Value > 0)
                employeesQuery = employeesQuery.Where(e => e.CompanyId == companyId.Value);

            if (departmentId.HasValue && departmentId.Value > 0)
                employeesQuery = employeesQuery.Where(e => e.DepartmentId == departmentId.Value);

            var allowedIds = await employeesQuery
                .Where(e => ids.Contains(e.Id))
                .Select(e => e.Id)
                .ToListAsync();

            leaveSummaries = leaveSummaries
                .Where(s => allowedIds.Contains(s.EmployeeId))
                .ToList();
        }

        var result = new List<PayrollLeaveRowDto>();

        foreach (var s in leaveSummaries)
        {
            decimal annual = 0,
                    paternity = 0,
                    maternity = 0,
                    sick = 0,
                    unpaid = 0,
                    familyResp = 0;

            foreach (var t in s.LeaveTypes)
            {
                switch (t.LeaveTypeName)
                {
                    case "Annual Leave":
                        annual = t.Remaining;
                        break;
                    case "Paternity Leave":
                        paternity = t.Remaining;
                        break;
                    case "Maternity Leave":
                        maternity = t.Remaining;
                        break;
                    case "Sick Leave":
                        sick = t.Remaining;
                        break;
                    case "Unpaid Leave":
                        unpaid = t.Remaining;
                        break;
                    case "Family Responsibility":
                        familyResp = t.Remaining;
                        break;
                }
            }

            result.Add(new PayrollLeaveRowDto
            {
                EmployeeId = s.EmployeeId,
                EmployeeName = s.EmployeeName,
                AnnualLeave = annual,
                PaternityLeave = paternity,
                MaternityLeave = maternity,
                SickLeave = sick,
                UnpaidLeave = unpaid,
                FamilyResponsibility = familyResp
            });
        }

        return result
            .OrderBy(r => r.EmployeeName)
            .ToList();
    }

    public async Task<List<SaturdayWorkSummary>> GetSaturdayWorkReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? employeeId)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        var employeesQuery = _context.Employees
            .Include(e => e.Department)
            .Where(e => e.IsActive);

        if (employeeId.HasValue && employeeId.Value > 0)
            employeesQuery = employeesQuery.Where(e => e.Id == employeeId.Value);

        var employees = await employeesQuery.ToListAsync();

        var startUtc = DateTime.SpecifyKind(startDate, DateTimeKind.Local).ToUniversalTime();
        var endUtc = DateTime.SpecifyKind(endDate.AddDays(1), DateTimeKind.Local).ToUniversalTime();

        var attendanceAllQuery = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Where(a => a.ClockInTime >= startUtc && a.ClockInTime < endUtc);

        if (employeeId.HasValue && employeeId.Value > 0)
            attendanceAllQuery = attendanceAllQuery.Where(a => a.EmployeeId == employeeId.Value);

        var attendanceAll = await attendanceAllQuery.ToListAsync();

        var saturdayAttendance = attendanceAll
            .Where(a => a.ClockInTime.ToLocalTime().DayOfWeek == DayOfWeek.Saturday)
            .ToList();

        var summaries = new List<SaturdayWorkSummary>();

        foreach (var emp in employees)
        {
            var dept = emp.Department;
            if (dept == null)
                continue;

            var expected = 0;
            if (dept.WorksSaturday)
            {
                for (var d = startDate; d <= endDate; d = d.AddDays(1))
                {
                    if (d.DayOfWeek == DayOfWeek.Saturday)
                        expected++;
                }
            }

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

// DTO used by new payroll endpoint
public class PayrollTimeSummaryDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public double NormalHours { get; set; }
    public double OvertimeWeekdayApproved { get; set; }
    public double OvertimeSundayApproved { get; set; }
    public double DriverApproved { get; set; }
}
