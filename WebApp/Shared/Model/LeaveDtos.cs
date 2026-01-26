using System;

namespace WebApp.Shared.Model
{
    public enum LeavePortion
    {
        FullDay = 1,
        HalfDay = 2
    }

    public class EmployeeLeaveDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal DaysBalance { get; set; }
        public decimal AccrualRatePerMonth { get; set; }
        public int DaysPerWeek { get; set; }
        public DateTime LastAccrualDate { get; set; }
    }

    public class LeaveRecordDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal DaysTaken { get; set; }
        public LeaveType LeaveType { get; set; }
        public LeaveStatus Status { get; set; }
        public string Reason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public LeavePortion Portion { get; set; }

        // New: attachment file name or relative URL
        public string? AttachmentFileName { get; set; }
    }

    public class RequestLeaveDto
    {
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public LeaveType LeaveType { get; set; }
        public string Reason { get; set; }
        public LeavePortion Portion { get; set; } = LeavePortion.FullDay;

        // Optional for API that echoes back data; the actual file travels separately
        public string? AttachmentFileName { get; set; }
    }

    public class LeaveSetupRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int DaysPerWeek { get; set; }
        public decimal AccrualRatePerMonth { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal NewBalance { get; set; }
    }
}
