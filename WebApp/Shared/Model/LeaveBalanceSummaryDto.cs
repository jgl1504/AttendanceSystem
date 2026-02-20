public class LeaveBalanceSummaryDto
{
    public string LeaveTypeName { get; set; } = string.Empty;
    public decimal TotalEntitlement { get; set; }
    public decimal Taken { get; set; }
    public decimal Remaining { get; set; }

    // NEW: when the opening balance is effective from (mainly for Annual)
    public DateTime? OpeningFromDate { get; set; }
    public decimal AccruedSinceStart { get; set; }
}
