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
        DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        var query =
            from a in _context.AttendanceRecords
            let workDate = a.ClockInTime.Date
            where workDate >= startDate && workDate <= endDate
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
        DateTime endDate)
    {
        startDate = startDate.Date;
        endDate = endDate.Date;

        // 1) Approved leave records that overlap the period
        var takenQuery =
            from r in _context.LeaveRecords   // EF entity matching LeaveRecordDto
            where r.Status == LeaveStatus.Approved
                  // any overlap with the selected period
                  && r.StartDate.Date <= endDate
                  && r.EndDate.Date >= startDate
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
}
