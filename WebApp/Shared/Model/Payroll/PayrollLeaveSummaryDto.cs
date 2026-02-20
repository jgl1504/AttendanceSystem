namespace WebApp.Shared.Model.Payroll;

public class PayrollLeaveSummaryDto
{
    public Guid LeaveTypeId { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;

    public decimal DaysTakenInMonth { get; set; }
    public decimal DaysTakenWeek1 { get; set; }
    public decimal DaysTakenWeek2 { get; set; }
    public decimal DaysTakenWeek3 { get; set; }
    public decimal DaysTakenWeek4 { get; set; }

    public decimal? CurrentBalance { get; set; } // null or 0 for pure-unpaid
}
