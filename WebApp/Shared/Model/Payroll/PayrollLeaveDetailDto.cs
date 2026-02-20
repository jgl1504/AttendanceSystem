namespace WebApp.Shared.Model.Payroll
{
    public class PayrollLeaveDetailDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Hours { get; set; }
        public string? Reason { get; set; } // Changed from Notes to Reason to match LeaveRecord
    }
}
