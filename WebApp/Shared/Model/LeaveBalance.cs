namespace WebApp.Shared.Model
{
    public class LeaveBalance : BaseEntity
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public Guid LeaveTypeId { get; set; }
        public LeaveType LeaveType { get; set; } = null!;

        // Starting balance setup
        public DateTime BalanceStartDate { get; set; }
        public decimal OpeningBalance { get; set; }

        // Running balance for this employee + leave type (days)
        public decimal CurrentBalance { get; set; }

        // For cycle-based leave (sick leave)
        public DateTime? CurrentCycleStartDate { get; set; }
        public DateTime? CurrentCycleEndDate { get; set; }

        // For one-time leave (maternity)
        public bool HasBeenUsed { get; set; } = false;
        public DateTime? UsedDate { get; set; }
    }
}
