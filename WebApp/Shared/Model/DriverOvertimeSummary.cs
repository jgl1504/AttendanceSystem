namespace WebApp.Shared.Model
{
    public class DriverOvertimeSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double NormalHours { get; set; }
        public double WeekdayOvertimeHours { get; set; }
        public double SundayPublicOvertimeHours { get; set; }

        public double ApprovedOvertimeHours { get; set; }
        public double UnapprovedOvertimeHours { get; set; }
    }
}
