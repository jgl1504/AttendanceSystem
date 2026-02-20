namespace WebApp.Shared.Model.Payroll
{
    public class PayrollHoursRowDto
    {
        public string EmployeeName { get; set; } = string.Empty;

        public double NormalHours { get; set; }
        public double OvertimeApproved { get; set; }
        public double OvertimeSundayApproved { get; set; }
        public double DriverApproved { get; set; }
    }
}
