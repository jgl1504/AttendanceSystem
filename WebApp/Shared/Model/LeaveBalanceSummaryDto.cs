namespace WebApp.Shared.Model
{
    public class LeaveBalanceSummaryDto
    {
        public string LeaveTypeName { get; set; } = string.Empty;
        public decimal TotalEntitlement { get; set; }
        public decimal Taken { get; set; }
        public decimal Remaining { get; set; }
    }
}
