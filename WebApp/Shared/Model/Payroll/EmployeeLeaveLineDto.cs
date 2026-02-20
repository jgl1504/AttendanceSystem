namespace WebApp.Shared.Model.Payroll
{ 
    public class EmployeeLeaveLineDto
    {
        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        public decimal TotalEntitlement { get; set; }
        public decimal TakenToDate { get; set; }
        public decimal Remaining { get; set; }
        public decimal TakenInMonth { get; set; }

        // NEW: debug fields
        public DateTime? EmployeeStartDate { get; set; }
        public DateTime? AnnualOpeningFromDate { get; set; }

        // NEW: total accrued days since accrual start
        public decimal AccruedSinceStart { get; set; }
    }
}
