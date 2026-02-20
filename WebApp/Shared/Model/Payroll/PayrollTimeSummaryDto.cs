namespace WebApp.Shared.Model.Payroll

{
    public class PayrollTimeSummaryDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;

        public decimal NormalHours { get; set; }
        public decimal OvertimeApproved { get; set; }
        public decimal OvertimeSundayApproved { get; set; }
        public decimal DriverApproved { get; set; }
    }
}