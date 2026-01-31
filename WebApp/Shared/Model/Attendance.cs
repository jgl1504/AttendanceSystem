namespace WebApp.Shared.Model
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int ClockedByEmployeeId { get; set; }
        public Employee ClockedByEmployee { get; set; } = null!;

        public DateTime ClockInTime { get; set; }
        public DateTime? ClockOutTime { get; set; }

        public double? ClockInLatitude { get; set; }
        public double? ClockInLongitude { get; set; }
        public double? ClockOutLatitude { get; set; }
        public double? ClockOutLongitude { get; set; }

        // Site selection
        public Guid? SiteId { get; set; }
        public Site? Site { get; set; }

        // New: segment category (Normal/Driver/Breakdown)
        public WorkCategory WorkCategory { get; set; } = WorkCategory.Normal;

        // Persisted hours
        public double? HoursWorked { get; set; }
        public double? OvertimeHours { get; set; }
        public double? WeekdayOvertimeHours { get; set; }
        public double? SundayPublicOvertimeHours { get; set; }

        // Overtime workflow
        public OvertimeStatus OvertimeStatus { get; set; } = OvertimeStatus.None;

        // Where overtime happened (client/site, e.g. "Dischem")
        public string? OvertimeLocation { get; set; }

        // Free-text note / reason from the approver
        public string? OvertimeNote { get; set; }

        // Who approved/rejected this overtime (logged-in approver)
        public int? OvertimeApprovedByEmployeeId { get; set; }
        public Employee? OvertimeApprovedByEmployee { get; set; }

        // When the decision was made
        public DateTime? OvertimeDecisionTime { get; set; }
    }
}
