namespace WebApp.Shared.Model
{
    public class LeaveSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Per leave type for the period
        public List<LeaveBalanceSummaryDto> LeaveTypes { get; set; } = new();
    }
}
