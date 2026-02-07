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
        // 1) Prefer any open record (no ClockOutTime), regardless of date
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

        // 2) No open record – fall back to last closed record
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
            SiteId = request.SiteId,  // Store selected site
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
            .Include(a => a.Site)  // Include site data
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

        // 1) Compute HoursWorked per segment (record) WITHOUT subtracting daily break,
        //    so short segments like 20:08–20:29 show their actual duration.
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

        // 2) Group by employee to compute DAILY totals
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
                    bool isPublicHoliday = false; // TODO

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

        // 3) Project per record DTO, attaching daily overtime info
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

                // Segment hours
                HoursWorked = a.HoursWorked,

                // DAILY expected/OT
                ExpectedHours = daily.ExpectedHours,
                OvertimeHours = daily.OvertimeHours,
                WeekdayOvertimeHours = daily.WeekdayOvertime,
                SundayPublicOvertimeHours = daily.SundayHolidayOvertime,
                OvertimeStatus = daily.Status,

                // Category info
                WorkCategory = a.WorkCategory,
                NormalHours = normalHours,
                DriverHours = driverHours,
                BreakdownHours = breakdownHours,

                // Site info
                SiteId = a.SiteId,
                SiteName = a.Site?.Name,

                // Overtime detail
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
                SiteId = null,  // Quick entry doesn't capture site
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

    public async Task<AttendanceRecord?> GetRecordForDecisionAsync(int id)
        => await _context.AttendanceRecords.FindAsync(id);

    public Task<int> SaveChangesAsync()
        => _context.SaveChangesAsync();
}
