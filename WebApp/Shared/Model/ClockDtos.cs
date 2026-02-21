namespace WebApp.Shared.Model
{
    public class ClockStatusDto
    {
        public bool IsClockedIn { get; set; }
        public DateTime? LastClockInTime { get; set; }
        public DateTime? LastClockOutTime { get; set; }
    }

    public class ClockRequestDto
    {
        public int EmployeeId { get; set; }    // employee being clocked
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // New: segment category (Normal / Driver / Breakdown)
        public WorkCategory WorkCategory { get; set; } = WorkCategory.Normal;

        // NEW: Selected site
        public Guid? SiteId { get; set; }
    }

    public class AttendanceListItemDto
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        public double? ClockInLatitude { get; set; }
        public double? ClockInLongitude { get; set; }
        public double? ClockOutLatitude { get; set; }
        public double? ClockOutLongitude { get; set; }

        public double? HoursWorked { get; set; }
        public double? ExpectedHours { get; set; }
        public double? OvertimeHours { get; set; }

        public double? WeekdayOvertimeHours { get; set; }
        public double? SundayPublicOvertimeHours { get; set; }

        public OvertimeStatus OvertimeStatus { get; set; }

        // New: category totals for the day (summary)
        public double? NormalHours { get; set; }
        public double? DriverHours { get; set; }
        public double? BreakdownHours { get; set; }

        // New: approved driver hours (separate from OT)
        public double? ApprovedDriverHours { get; set; }

        // New: category of this record if you show segments in a flat list
        public WorkCategory WorkCategory { get; set; } = WorkCategory.Normal;

        // Site
        public Guid? SiteId { get; set; }
        public string? SiteName { get; set; }

        // Overtime detail fields for approval UI
        public string? OvertimeLocation { get; set; }
        public string? OvertimeNote { get; set; }
        public string? OvertimeApprovedByName { get; set; }
        public DateTime? OvertimeDecisionTime { get; set; }

        // Convenience flags for the UI
        public bool HasOvertime => (OvertimeHours ?? 0) > 0;
        public bool IsOvertimeApproved => OvertimeStatus == OvertimeStatus.Approved;
        public bool IsOvertimePending => OvertimeStatus == OvertimeStatus.Pending;
    }



    public class AttendanceEditDto
    {
        public int Id { get; set; }
        public DateTime ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        // Let admin change the category on a segment
        public WorkCategory WorkCategory { get; set; } = WorkCategory.Normal;
    }

    public enum OvertimeStatus
    {
        None = 0,      // no overtime
        Pending = 1,   // has overtime, waiting for decision
        Approved = 2,
        Denied = 3
    }

    public class IdDto
    {
        public int Id { get; set; }
    }

    public class QuickEntryRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int? DepartmentId { get; set; }
        public int? AttendanceId { get; set; }
        public DateTime? ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }
        public string DefaultClockIn { get; set; } = "";
        public string DefaultClockOut { get; set; } = "";

        // Optional: default category for quick entry
        public WorkCategory WorkCategory { get; set; } = WorkCategory.Normal;
    }

    public class SaveQuickEntryDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public string? ClockInTime { get; set; }
        public string? ClockOutTime { get; set; }

        public WorkCategory WorkCategory { get; set; } = WorkCategory.Normal;
    }

    public class SaveQuickEntryBatchDto
    {
        public DateTime Date { get; set; }
        public List<SaveQuickEntryDto> Entries { get; set; } = new();
    }
}
