using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Attendance;

public class AttendanceService
{
    private readonly DataContext _context;

    public AttendanceService(DataContext context)
    {
        _context = context;
    }

    public async Task<ClockStatusDto> GetStatusAsync(int employeeId)
    {
        var openRecord = await _context.AttendanceRecords
            .Where(a => a.EmployeeId == employeeId && a.ClockOutTime == null)
            .OrderByDescending(a => a.ClockInTime)
            .FirstOrDefaultAsync();

        if (openRecord is not null)
        {
            return new ClockStatusDto
            {
                IsClockedIn = true,
                LastClockInTime = openRecord.ClockInTime,
                LastClockOutTime = null
            };
        }

        var lastRecord = await _context.AttendanceRecords
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.ClockInTime)
            .FirstOrDefaultAsync();

        if (lastRecord is null)
        {
            return new ClockStatusDto
            {
                IsClockedIn = false
            };
        }

        return new ClockStatusDto
        {
            IsClockedIn = false,
            LastClockInTime = lastRecord.ClockInTime,
            LastClockOutTime = lastRecord.ClockOutTime
        };
    }

    public async Task<bool> ClockInAsync(ClockRequestDto request, int clockedByEmployeeId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        var openRecord = await _context.AttendanceRecords
            .Where(a => a.EmployeeId == request.EmployeeId && a.ClockOutTime == null)
            .FirstOrDefaultAsync();

        if (openRecord is not null)
        {
            return false;
        }

        var record = new AttendanceRecord
        {
            EmployeeId = request.EmployeeId,
            ClockedByEmployeeId = clockedByEmployeeId,
            ClockInTime = DateTime.UtcNow,
            ClockInLatitude = request.Latitude,
            ClockInLongitude = request.Longitude,
            WorkCategory = request.WorkCategory,
            SiteId = request.SiteId,
            OvertimeStatus = OvertimeStatus.None,
            OvertimeLocation = null,
            OvertimeNote = null,
            OvertimeApprovedByEmployeeId = null,
            OvertimeApprovedByEmployee = null,
            OvertimeDecisionTime = null
        };

        _context.AttendanceRecords.Add(record);

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }

    public async Task<bool> ClockOutAsync(ClockRequestDto request, int clockedByEmployeeId)
    {
        var openRecord = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId &&
                                      a.ClockOutTime == null);

        if (openRecord is null)
            return false;

        openRecord.ClockOutTime = DateTime.UtcNow;
        openRecord.ClockOutLatitude = request.Latitude;
        openRecord.ClockOutLongitude = request.Longitude;
        openRecord.ClockedByEmployeeId = clockedByEmployeeId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AttendanceListItemDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await GetByDateInternalAsync(today, null, null);
    }

    public async Task<List<AttendanceListItemDto>> GetByDateAsync(DateTime date, int? employeeId, int? departmentId)
    {
        var day = date.Date;
        return await GetByDateInternalAsync(day, employeeId, departmentId);
    }

    private async Task<List<AttendanceListItemDto>> GetByDateInternalAsync(
        DateTime dayLocal,
        int? employeeId,
        int? departmentId)
    {
        var startLocal = dayLocal.Date;
        var endLocal = startLocal.AddDays(1);

        var startUtc = DateTime.SpecifyKind(startLocal, DateTimeKind.Local).ToUniversalTime();
        var endUtc = DateTime.SpecifyKind(endLocal, DateTimeKind.Local).ToUniversalTime();

        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.OvertimeApprovedByEmployee)
            .Include(a => a.Site)
            .Where(a => a.ClockInTime >= startUtc && a.ClockInTime < endUtc);

        if (employeeId.HasValue && employeeId.Value > 0)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);
        }

        var records = await query
            .OrderBy(a => a.ClockInTime)
            .ToListAsync();

        foreach (var a in records)
        {
            double? hoursWorked = null;

            if (a.ClockOutTime.HasValue)
            {
                var total = (a.ClockOutTime.Value - a.ClockInTime).TotalHours;
                hoursWorked = Math.Round(Math.Max(0, total), 2);
            }

            a.HoursWorked = hoursWorked;
        }

        var dailyGroups = records
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.First();
                    var dept = first.Employee.Department!;

                    var anyClockIn = first.ClockInTime.ToLocalTime();
                    var dayOfWeek = anyClockIn.DayOfWeek;

                    var weekdayExpectedPerDay = dept.RequiredHoursPerWeek / 5m;

                    bool isSunday = dayOfWeek == DayOfWeek.Sunday;
                    bool isSaturday = dayOfWeek == DayOfWeek.Saturday;
                    bool isPublicHoliday = false;

                    double expectedHours;

                    if (isSunday || isPublicHoliday)
                    {
                        if (dept.WorksSunday)
                            expectedHours = (double)dept.SundayHours;
                        else
                            expectedHours = 0.0;
                    }
                    else if (isSaturday)
                    {
                        if (dept.WorksSaturday)
                            expectedHours = (double)dept.SaturdayHours;
                        else
                            expectedHours = 0.0;
                    }
                    else
                    {
                        expectedHours = (double)weekdayExpectedPerDay;
                    }

                    var totalHoursForDay = g.Sum(x => x.HoursWorked ?? 0);
                    var rawOvertime = Math.Max(0, totalHoursForDay - expectedHours);

                    var anyApproved = g.Any(x => x.OvertimeStatus == OvertimeStatus.Approved);
                    var anyDenied = g.Any(x => x.OvertimeStatus == OvertimeStatus.Denied);
                    OvertimeStatus status;
                    if (rawOvertime <= 0)
                    {
                        status = OvertimeStatus.None;
                    }
                    else if (anyApproved)
                    {
                        status = OvertimeStatus.Approved;
                    }
                    else if (anyDenied)
                    {
                        status = OvertimeStatus.Denied;
                    }
                    else
                    {
                        status = OvertimeStatus.Pending;
                    }

                    double weekdayOt = 0;
                    double sundayHolidayOt = 0;

                    if (rawOvertime > 0)
                    {
                        if (isSunday || isPublicHoliday)
                            sundayHolidayOt = rawOvertime;
                        else
                            weekdayOt = rawOvertime;
                    }

                    return new
                    {
                        ExpectedHours = Math.Round(expectedHours, 2),
                        TotalHours = Math.Round(totalHoursForDay, 2),
                        OvertimeHours = Math.Round(rawOvertime, 2),
                        WeekdayOvertime = Math.Round(weekdayOt, 2),
                        SundayHolidayOvertime = Math.Round(sundayHolidayOt, 2),
                        Status = status
                    };
                });

        var list = new List<AttendanceListItemDto>();

        foreach (var a in records)
        {
            var daily = dailyGroups[a.EmployeeId];

            a.OvertimeHours = daily.OvertimeHours;
            a.WeekdayOvertimeHours = daily.WeekdayOvertime;
            a.SundayPublicOvertimeHours = daily.SundayHolidayOvertime;
            a.OvertimeStatus = daily.Status;

            double? normalHours = null;
            double? driverHours = null;
            double? breakdownHours = null;

            if (a.WorkCategory == WorkCategory.Normal)
                normalHours = a.HoursWorked;
            else if (a.WorkCategory == WorkCategory.Driver)
                driverHours = a.HoursWorked;
            else if (a.WorkCategory == WorkCategory.Breakdown)
                breakdownHours = a.HoursWorked;

            list.Add(new AttendanceListItemDto
            {
                Id = a.Id,
                EmployeeName = a.Employee.Name,
                ClockInTime = a.ClockInTime,
                ClockOutTime = a.ClockOutTime,
                ClockInLatitude = a.ClockInLatitude,
                ClockInLongitude = a.ClockInLongitude,
                ClockOutLatitude = a.ClockOutLatitude,
                ClockOutLongitude = a.ClockOutLongitude,

                HoursWorked = a.HoursWorked,

                ExpectedHours = daily.ExpectedHours,
                OvertimeHours = daily.OvertimeHours,
                WeekdayOvertimeHours = daily.WeekdayOvertime,
                SundayPublicOvertimeHours = daily.SundayHolidayOvertime,
                OvertimeStatus = daily.Status,

                WorkCategory = a.WorkCategory,
                NormalHours = normalHours,
                DriverHours = driverHours,
                BreakdownHours = breakdownHours,

                SiteId = a.SiteId,
                SiteName = a.Site?.Name,

                OvertimeLocation = a.OvertimeLocation,
                OvertimeNote = a.OvertimeNote,
                OvertimeApprovedByName = a.OvertimeApprovedByEmployee != null
                    ? a.OvertimeApprovedByEmployee.Name
                    : null,
                OvertimeDecisionTime = a.OvertimeDecisionTime
            });
        }

        await _context.SaveChangesAsync();
        return list;
    }

    public async Task<bool> UpdateTimesAsync(AttendanceEditDto dto)
    {
        var record = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.Id == dto.Id);

        if (record is null)
            return false;

        record.ClockInTime = dto.ClockInTime;
        record.ClockOutTime = dto.ClockOutTime;
        record.WorkCategory = dto.WorkCategory;

        record.OvertimeStatus = OvertimeStatus.None;
        record.OvertimeLocation = null;
        record.OvertimeNote = null;
        record.OvertimeApprovedByEmployeeId = null;
        record.OvertimeDecisionTime = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record is null) return false;

        _context.AttendanceRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ApproveOvertimeAsync(int id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record is null) return false;

        if ((record.HoursWorked ?? 0) <= 0 || record.ClockOutTime is null)
            return false;

        record.OvertimeStatus = OvertimeStatus.Approved;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DenyOvertimeAsync(int id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record is null) return false;

        if ((record.HoursWorked ?? 0) <= 0 || record.ClockOutTime is null)
            return false;

        record.OvertimeStatus = OvertimeStatus.Denied;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetOvertimeAsync(int id)
    {
        var record = await _context.AttendanceRecords.FindAsync(id);
        if (record is null) return false;

        if ((record.HoursWorked ?? 0) <= 0 || record.ClockOutTime is null)
            return false;

        record.OvertimeStatus = OvertimeStatus.Pending;
        record.OvertimeLocation = null;
        record.OvertimeNote = null;
        record.OvertimeApprovedByEmployeeId = null;
        record.OvertimeDecisionTime = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<QuickEntryRowDto>> GetQuickEntryDataAsync(DateTime date)
    {
        var dayLocal = date.Date;
        var startUtc = DateTime.SpecifyKind(dayLocal, DateTimeKind.Local).ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        var employees = await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToListAsync();

        var attendanceRecords = await _context.AttendanceRecords
            .Include(a => a.Site)
            .Where(a => a.ClockInTime >= startUtc && a.ClockInTime < endUtc)
            .ToListAsync();

        var rows = new List<QuickEntryRowDto>();

        foreach (var emp in employees)
        {
            var record = attendanceRecords.FirstOrDefault(a => a.EmployeeId == emp.Id);
            var dept = emp.Department;

            string defaultClockIn = "";
            string defaultClockOut = "";

            if (dept != null)
            {
                var dayOfWeek = date.DayOfWeek;
                bool isSunday = dayOfWeek == DayOfWeek.Sunday;
                bool isSaturday = dayOfWeek == DayOfWeek.Saturday;

                if (isSunday)
                {
                    if (dept.WorksSunday)
                    {
                        defaultClockIn = dept.DailyStartTime.ToString(@"hh\\:mm");
                        defaultClockOut = dept.DailyEndTime.ToString(@"hh\\:mm");
                    }
                }
                else if (isSaturday)
                {
                    if (dept.WorksSaturday)
                    {
                        defaultClockIn = dept.DailyStartTime.ToString(@"hh\\:mm");
                        defaultClockOut = dept.DailyEndTime.ToString(@"hh\\:mm");
                    }
                }
                else
                {
                    defaultClockIn = dept.DailyStartTime.ToString(@"hh\\:mm");
                    defaultClockOut = dept.DailyEndTime.ToString(@"hh\\:mm");
                }
            }

            rows.Add(new QuickEntryRowDto
            {
                EmployeeId = emp.Id,
                EmployeeName = emp.Name,
                DepartmentName = dept?.Name ?? "",
                DepartmentId = dept?.Id,
                AttendanceId = record?.Id,
                ClockInTime = record?.ClockInTime,
                ClockOutTime = record?.ClockOutTime,
                DefaultClockIn = defaultClockIn,
                DefaultClockOut = defaultClockOut,
                WorkCategory = record?.WorkCategory ?? WorkCategory.Normal
            });
        }

        return rows;
    }

    public async Task<bool> SaveQuickEntryAsync(int employeeId, DateTime date, string? clockInTime, string? clockOutTime, int clockedByEmployeeId)
    {
        var dayLocal = date.Date;
        var startUtc = DateTime.SpecifyKind(dayLocal, DateTimeKind.Local).ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        var existing = await _context.AttendanceRecords
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId &&
                                      a.ClockInTime >= startUtc &&
                                      a.ClockInTime < endUtc);

        DateTime? clockInUtc = null;
        DateTime? clockOutUtc = null;

        if (!string.IsNullOrWhiteSpace(clockInTime) && TimeSpan.TryParse(clockInTime, out var inTime))
        {
            clockInUtc = (dayLocal.Date + inTime).ToUniversalTime();
        }

        if (!string.IsNullOrWhiteSpace(clockOutTime) && TimeSpan.TryParse(clockOutTime, out var outTime))
        {
            clockOutUtc = (dayLocal.Date + outTime).ToUniversalTime();
        }

        if (existing != null)
        {
            if (clockInUtc.HasValue)
                existing.ClockInTime = clockInUtc.Value;
            if (clockOutUtc.HasValue)
                existing.ClockOutTime = clockOutUtc;
            else
                existing.ClockOutTime = null;

            existing.ClockedByEmployeeId = clockedByEmployeeId;
            existing.OvertimeStatus = OvertimeStatus.None;
            existing.OvertimeLocation = null;
            existing.OvertimeNote = null;
            existing.OvertimeApprovedByEmployeeId = null;
            existing.OvertimeDecisionTime = null;
        }
        else
        {
            if (!clockInUtc.HasValue)
                return false;

            var record = new AttendanceRecord
            {
                EmployeeId = employeeId,
                ClockedByEmployeeId = clockedByEmployeeId,
                ClockInTime = clockInUtc.Value,
                ClockOutTime = clockOutUtc,
                WorkCategory = WorkCategory.Normal,
                SiteId = null,
                OvertimeStatus = OvertimeStatus.None,
                OvertimeLocation = null,
                OvertimeNote = null,
                OvertimeApprovedByEmployeeId = null,
                OvertimeDecisionTime = null
            };

            _context.AttendanceRecords.Add(record);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearTodayRecordAsync(int employeeId)
    {
        var today = DateTime.UtcNow.Date;
        var startUtc = DateTime.SpecifyKind(today, DateTimeKind.Local).ToUniversalTime();
        var endUtc = startUtc.AddDays(1);

        var todayRecords = await _context.AttendanceRecords
            .Where(a => a.EmployeeId == employeeId &&
                        a.ClockInTime >= startUtc &&
                        a.ClockInTime < endUtc)
            .ToListAsync();

        if (!todayRecords.Any())
            return false;

        _context.AttendanceRecords.RemoveRange(todayRecords);
        await _context.SaveChangesAsync();
        return true;
    }

    // UPDATED: added companyId filter parameter
    public async Task<List<AttendanceListItemDto>> GetByDateRangeAsync(
        DateTime fromLocalInclusive,
        DateTime toLocalExclusive,
        int? employeeId,
        int? departmentId,
        int? companyId)   // <-- NEW
    {
        var startUtc = DateTime.SpecifyKind(fromLocalInclusive.Date, DateTimeKind.Local).ToUniversalTime();
        var endUtc = DateTime.SpecifyKind(toLocalExclusive.Date, DateTimeKind.Local).ToUniversalTime();

        var query = _context.AttendanceRecords
            .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
            .Include(a => a.Employee)              // ensure Employee is included once
                .ThenInclude(e => e.Company)       // <-- ensure Company is loaded
            .Include(a => a.OvertimeApprovedByEmployee)
            .Include(a => a.Site)
            .Where(a => a.ClockInTime >= startUtc && a.ClockInTime < endUtc);

        if (employeeId.HasValue && employeeId.Value > 0)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(a => a.Employee.DepartmentId == departmentId.Value);
        }

        if (companyId.HasValue && companyId.Value > 0)
        {
            query = query.Where(a => a.Employee.CompanyId == companyId.Value);   // <-- filter by company
        }

        var records = await query
            .OrderBy(a => a.ClockInTime)
            .ToListAsync();

        foreach (var a in records)
        {
            double? hoursWorked = null;
            if (a.ClockOutTime.HasValue)
            {
                var total = (a.ClockOutTime.Value - a.ClockInTime).TotalHours;
                hoursWorked = Math.Round(Math.Max(0, total), 2);
            }
            a.HoursWorked = hoursWorked;
        }

        var dailyGroups = records
            .GroupBy(a => new { a.EmployeeId, Day = a.ClockInTime.ToLocalTime().Date })
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.First();
                    var dept = first.Employee.Department!;

                    var anyClockIn = first.ClockInTime.ToLocalTime();
                    var dayOfWeek = anyClockIn.DayOfWeek;

                    var weekdayExpectedPerDay = dept.RequiredHoursPerWeek / 5m;

                    bool isSunday = dayOfWeek == DayOfWeek.Sunday;
                    bool isSaturday = dayOfWeek == DayOfWeek.Saturday;
                    bool isPublicHoliday = false;

                    double expectedHours;

                    if (isSunday || isPublicHoliday)
                    {
                        if (dept.WorksSunday)
                            expectedHours = (double)dept.SundayHours;
                        else
                            expectedHours = 0.0;
                    }
                    else if (isSaturday)
                    {
                        if (dept.WorksSaturday)
                            expectedHours = (double)dept.SaturdayHours;
                        else
                            expectedHours = 0.0;
                    }
                    else
                    {
                        expectedHours = (double)weekdayExpectedPerDay;
                    }

                    var totalHoursForDay = g.Sum(x => x.HoursWorked ?? 0);
                    var rawOvertime = Math.Max(0, totalHoursForDay - expectedHours);

                    var anyApproved = g.Any(x => x.OvertimeStatus == OvertimeStatus.Approved);
                    var anyDenied = g.Any(x => x.OvertimeStatus == OvertimeStatus.Denied);
                    OvertimeStatus status;
                    if (rawOvertime <= 0)
                    {
                        status = OvertimeStatus.None;
                    }
                    else if (anyApproved)
                    {
                        status = OvertimeStatus.Approved;
                    }
                    else if (anyDenied)
                    {
                        status = OvertimeStatus.Denied;
                    }
                    else
                    {
                        status = OvertimeStatus.Pending;
                    }

                    double weekdayOt = 0;
                    double sundayHolidayOt = 0;

                    if (rawOvertime > 0)
                    {
                        if (isSunday || isPublicHoliday)
                            sundayHolidayOt = rawOvertime;
                        else
                            weekdayOt = rawOvertime;
                    }

                    return new
                    {
                        ExpectedHours = Math.Round(expectedHours, 2),
                        TotalHours = Math.Round(totalHoursForDay, 2),
                        OvertimeHours = Math.Round(rawOvertime, 2),
                        WeekdayOvertime = Math.Round(weekdayOt, 2),
                        SundayHolidayOvertime = Math.Round(sundayHolidayOt, 2),
                        Status = status
                    };
                });

        var list = new List<AttendanceListItemDto>();

        foreach (var a in records)
        {
            var dayKey = new { a.EmployeeId, Day = a.ClockInTime.ToLocalTime().Date };
            var daily = dailyGroups[dayKey];

            a.OvertimeHours = daily.OvertimeHours;
            a.WeekdayOvertimeHours = daily.WeekdayOvertime;
            a.SundayPublicOvertimeHours = daily.SundayHolidayOvertime;
            a.OvertimeStatus = daily.Status;

            double? normalHours = null;
            double? driverHours = null;
            double? breakdownHours = null;

            if (a.WorkCategory == WorkCategory.Normal)
                normalHours = a.HoursWorked;
            else if (a.WorkCategory == WorkCategory.Driver)
                driverHours = a.HoursWorked;
            else if (a.WorkCategory == WorkCategory.Breakdown)
                breakdownHours = a.HoursWorked;

            list.Add(new AttendanceListItemDto
            {
                Id = a.Id,
                EmployeeName = a.Employee.Name,
                ClockInTime = a.ClockInTime,
                ClockOutTime = a.ClockOutTime,
                ClockInLatitude = a.ClockInLatitude,
                ClockInLongitude = a.ClockInLongitude,
                ClockOutLatitude = a.ClockOutLatitude,
                ClockOutLongitude = a.ClockOutLongitude,

                HoursWorked = a.HoursWorked,

                ExpectedHours = daily.ExpectedHours,
                OvertimeHours = daily.OvertimeHours,
                WeekdayOvertimeHours = daily.WeekdayOvertime,
                SundayPublicOvertimeHours = daily.SundayHolidayOvertime,
                OvertimeStatus = daily.Status,

                WorkCategory = a.WorkCategory,
                NormalHours = normalHours,
                DriverHours = driverHours,
                BreakdownHours = breakdownHours,

                SiteId = a.SiteId,
                SiteName = a.Site?.Name,

                OvertimeLocation = a.OvertimeLocation,
                OvertimeNote = a.OvertimeNote,
                OvertimeApprovedByName = a.OvertimeApprovedByEmployee != null
                    ? a.OvertimeApprovedByEmployee.Name
                    : null,
                OvertimeDecisionTime = a.OvertimeDecisionTime
            });
        }

        await _context.SaveChangesAsync();
        return list;
    }

    public async Task<AttendanceRecord?> GetRecordForDecisionAsync(int id)
        => await _context.AttendanceRecords.FindAsync(id);

    public Task<int> SaveChangesAsync()
        => _context.SaveChangesAsync();

    // ===== LEAVE MONTHLY REPORT =====

    public enum LeavePool
    {
        Annual,
        Sick,
        Special,
        Unpaid
    }

    private static LeavePool MapLeaveTypeToPool(string leaveTypeName)
    {
        var name = leaveTypeName.Trim().ToLowerInvariant();

        if (name.Contains("annual"))
            return LeavePool.Annual;

        if (name.Contains("sick"))
            return LeavePool.Sick;

        if (name.Contains("unpaid"))
            return LeavePool.Unpaid;

        return LeavePool.Special;
    }

    private static (decimal annual, decimal sick, decimal special, decimal unpaid) AddToPools(
        decimal days,
        LeavePool pool,
        decimal annual,
        decimal sick,
        decimal special,
        decimal unpaid)
    {
        switch (pool)
        {
            case LeavePool.Annual:
                annual += days;
                break;
            case LeavePool.Sick:
                sick += days;
                break;
            case LeavePool.Special:
                special += days;
                break;
            case LeavePool.Unpaid:
                unpaid += days;
                break;
        }

        return (annual, sick, special, unpaid);
    }

    public async Task<List<LeaveMonthlyReportRowDto>> GetLeaveMonthlyReportAsync(
        int year,
        int? employeeId,
        int? departmentId)
    {
        var query = _context.LeaveRecords
            .Include(l => l.Employee)
                .ThenInclude(e => e.Department)
            .Include(l => l.LeaveType)
            .Where(l =>
                l.Status == LeaveStatus.Approved &&
                l.StartDate.Year == year);

        if (employeeId.HasValue && employeeId.Value > 0)
        {
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(l => l.Employee.DepartmentId == departmentId.Value);
        }

        var records = await query.ToListAsync();

        var empGroups = records
            .GroupBy(l => new { l.EmployeeId, l.Employee.Name })
            .ToList();

        var result = new List<LeaveMonthlyReportRowDto>();

        foreach (var emp in empGroups)
        {
            var row = new LeaveMonthlyReportRowDto
            {
                EmployeeId = emp.Key.EmployeeId,
                EmployeeName = emp.Key.Name
            };

            decimal balanceAnnual = 0;
            decimal balanceSick = 0;
            decimal balanceSpecial = 0;
            decimal balanceUnpaid = 0;

            var monthly = emp
                .Select(r => new
                {
                    Month = r.StartDate.Month,
                    Pool = MapLeaveTypeToPool(r.LeaveType.Name),
                    r.DaysTaken
                })
                .GroupBy(x => new { x.Month, x.Pool })
                .Select(g => new
                {
                    g.Key.Month,
                    g.Key.Pool,
                    Days = g.Sum(x => x.DaysTaken)
                })
                .ToList();

            foreach (var m in monthly)
            {
                switch (m.Month)
                {
                    case 1:
                        (row.JanAnnual, row.JanSick, row.JanSpecial, row.JanUnpaid) =
                            AddToPools(m.Days, m.Pool, row.JanAnnual, row.JanSick, row.JanSpecial, row.JanUnpaid);
                        break;
                    case 2:
                        (row.FebAnnual, row.FebSick, row.FebSpecial, row.FebUnpaid) =
                            AddToPools(m.Days, m.Pool, row.FebAnnual, row.FebSick, row.FebSpecial, row.FebUnpaid);
                        break;
                    case 3:
                        (row.MarAnnual, row.MarSick, row.MarSpecial, row.MarUnpaid) =
                            AddToPools(m.Days, m.Pool, row.MarAnnual, row.MarSick, row.MarSpecial, row.MarUnpaid);
                        break;
                    case 4:
                        (row.AprAnnual, row.AprSick, row.AprSpecial, row.AprUnpaid) =
                            AddToPools(m.Days, m.Pool, row.AprAnnual, row.AprSick, row.AprSpecial, row.AprUnpaid);
                        break;
                    case 5:
                        (row.MayAnnual, row.MaySick, row.MaySpecial, row.MayUnpaid) =
                            AddToPools(m.Days, m.Pool, row.MayAnnual, row.MaySick, row.MaySpecial, row.MayUnpaid);
                        break;
                    case 6:
                        (row.JunAnnual, row.JunSick, row.JunSpecial, row.JunUnpaid) =
                            AddToPools(m.Days, m.Pool, row.JunAnnual, row.JunSick, row.JunSpecial, row.JunUnpaid);
                        break;
                    case 7:
                        (row.JulAnnual, row.JulSick, row.JulSpecial, row.JulUnpaid) =
                            AddToPools(m.Days, m.Pool, row.JulAnnual, row.JulSick, row.JulSpecial, row.JulUnpaid);
                        break;
                    case 8:
                        (row.AugAnnual, row.AugSick, row.AugSpecial, row.AugUnpaid) =
                            AddToPools(m.Days, m.Pool, row.AugAnnual, row.AugSick, row.AugSpecial, row.AugUnpaid);
                        break;
                    case 9:
                        (row.SepAnnual, row.SepSick, row.SepSpecial, row.SepUnpaid) =
                            AddToPools(m.Days, m.Pool, row.SepAnnual, row.SepSick, row.SepSpecial, row.SepUnpaid);
                        break;
                    case 10:
                        (row.OctAnnual, row.OctSick, row.OctSpecial, row.OctUnpaid) =
                            AddToPools(m.Days, m.Pool, row.OctAnnual, row.OctSick, row.OctSpecial, row.OctUnpaid);
                        break;
                    case 11:
                        (row.NovAnnual, row.NovSick, row.NovSpecial, row.NovUnpaid) =
                            AddToPools(m.Days, m.Pool, row.NovAnnual, row.NovSick, row.NovSpecial, row.NovUnpaid);
                        break;
                    case 12:
                        (row.DecAnnual, row.DecSick, row.DecSpecial, row.DecUnpaid) =
                            AddToPools(m.Days, m.Pool, row.DecAnnual, row.DecSick, row.DecSpecial, row.DecUnpaid);
                        break;
                }

                switch (m.Pool)
                {
                    case LeavePool.Annual:
                        balanceAnnual += m.Days;
                        break;
                    case LeavePool.Sick:
                        balanceSick += m.Days;
                        break;
                    case LeavePool.Special:
                        balanceSpecial += m.Days;
                        break;
                    case LeavePool.Unpaid:
                        balanceUnpaid += m.Days;
                        break;
                }
            }

            row.BalanceAnnual = balanceAnnual;
            row.BalanceSick = balanceSick;
            row.BalanceSpecial = balanceSpecial;
            row.BalanceUnpaid = balanceUnpaid;

            result.Add(row);
        }

        return result
            .OrderBy(r => r.EmployeeName)
            .ToList();
    }
}
