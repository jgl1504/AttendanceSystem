using System.Text.Json.Serialization;

namespace WebApp.Shared.Model
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public decimal RequiredHoursPerWeek { get; set; }

        public TimeSpan DailyStartTime { get; set; } = new TimeSpan(8, 0, 0);   // 08:00
        public TimeSpan DailyEndTime { get; set; } = new TimeSpan(17, 0, 0);  // 17:00
        public TimeSpan BreakPerDay { get; set; } = new TimeSpan(1, 0, 0);   // 01:00

        public bool WorksSaturday { get; set; }
        public bool RotatingWeekends { get; set; }
        public int SaturdaysPerMonthRequired { get; set; }

        public decimal SaturdayHours { get; set; } = 5m;
        public bool WorksSunday { get; set; }
        public decimal SundayHours { get; set; } = 0m;

        public int GraceMinutesBefore { get; set; }
        public int GraceMinutesAfter { get; set; }
        public bool AllowOvertime { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }

}
