namespace WebApp.Shared.Model.Payroll
{
    public class PayrollLeaveRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public decimal AnnualLeave { get; set; }
        public decimal PaternityLeave { get; set; }
        public decimal MaternityLeave { get; set; }
        public decimal SickLeave { get; set; }
        public decimal UnpaidLeave { get; set; }
        public decimal FamilyResponsibility { get; set; }
    }

}
