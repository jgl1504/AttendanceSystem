namespace WebApp.Shared.Model.Payroll
{
    public class LeaveTypeBalanceRowDto
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        public decimal OpeningBalance { get; set; }   // days at start of month
        public decimal AccruedInMonth { get; set; }   // days accrued in month (0 for now if not used)
        public decimal TakenInMonth { get; set; }     // days taken in selected month
        public decimal CurrentBalance { get; set; }   // balance as at end of month
    }
}
