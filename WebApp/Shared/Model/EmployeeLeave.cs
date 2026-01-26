using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Shared.Model
{
    // Add to your shared models
    public class EmployeeLeave
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        // Current balance (can be positive or negative)
        public decimal DaysBalance { get; set; } // e.g., +5.5, -2.0

        // Configuration
        public decimal AccrualRatePerMonth { get; set; } // 1.25 for 5-day week, 1.5 for 6-day
        public int DaysPerWeek { get; set; } // 5 or 6

        // Tracking
        public DateTime LastAccrualDate { get; set; }
        public int LastAccrualMonth { get; set; } // Track which month was accrued

        // Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class LeaveRecord
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal DaysTaken { get; set; } // e.g., 5.0

        public LeavePortion Portion { get; set; } = LeavePortion.FullDay;

        public LeaveType LeaveType { get; set; }
        public LeaveStatus Status { get; set; }

        public string Reason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        // NEW: stored file name for doctor's letter / proof
        public string? AttachmentFileName { get; set; }
    }


    public enum LeaveType
    {
        Annual = 1,
        Sick = 2,
        FamilyResponsibility = 3,
        Maternity = 4,
        Other = 5
    }

    public enum LeaveStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Taken = 4,
        Cancelled = 5
    }
}
