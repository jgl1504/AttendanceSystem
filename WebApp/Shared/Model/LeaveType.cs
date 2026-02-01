namespace WebApp.Shared.Model
{
    public class LeaveType : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ColorCode { get; set; } = "#3498db";  // For calendar display
        public bool IsActive { get; set; } = true;

        // Pool Configuration
        public LeavePoolType PoolType { get; set; } = LeavePoolType.OwnPool;
        public Guid? PrimaryPoolLeaveTypeId { get; set; }
        public LeaveType? PrimaryPoolLeaveType { get; set; }
        public Guid? FallbackPoolLeaveTypeId { get; set; }
        public LeaveType? FallbackPoolLeaveType { get; set; }

        // Accrual Rules
        public LeaveAccrualType AccrualType { get; set; } = LeaveAccrualType.Annual;
        public decimal DaysPerYear { get; set; } = 15;
        public int? AccrualCycleDurationMonths { get; set; }  // For cycle-based (e.g., 36 months)
        public decimal? DaysPerCycle { get; set; }  // For cycle-based (e.g., 30 days)

        public bool AllowsCarryover { get; set; } = true;
        public int MaxCarryoverDays { get; set; } = 5;

        // Request Rules
        public bool RequiresSupportingDocument { get; set; } = false;
        public bool RequiresApproval { get; set; } = true;
        public int MinNoticeDays { get; set; } = 0;
        public int MaxConsecutiveDays { get; set; } = 0;  // 0 = unlimited
        public bool AllowsHalfDays { get; set; } = true;

        // Payment Rules
        public bool IsPaid { get; set; } = true;
        public decimal PaymentPercentage { get; set; } = 100;

        // Gender/Special Rules
        public bool IsGenderSpecific { get; set; } = false;
        public Gender? RequiredGender { get; set; }

        // Display
        public int SortOrder { get; set; } = 0;
    }

    public enum LeavePoolType
    {
        OwnPool = 0,           // Has its own balance (Annual, Sick)
        UsesOtherPool = 1,     // Deducts from another type (Study → Annual)
        Unlimited = 2,         // No pool tracking (Bereavement)
        OneTime = 3            // One-time allocation (Maternity)
    }

    public enum LeaveAccrualType
    {
        Annual = 0,      // Days per year (Annual Leave)
        Cycle = 1,       // Days per X months (Sick Leave: 30 days/36 months)
        Fixed = 2,       // One-time fixed amount (Maternity: 120 days)
        Unlimited = 3,   // No limit (Bereavement)
        None = 4         // No accrual (Unpaid)
    }

    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2
    }
}
