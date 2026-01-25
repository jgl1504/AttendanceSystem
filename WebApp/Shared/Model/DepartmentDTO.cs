using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Shared.Model
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public decimal RequiredHoursPerWeek { get; set; }

        // Strings in DTO for simple binding, converted to TimeSpan in the API
        public string? DailyStartTime { get; set; }
        public string? DailyEndTime { get; set; }
        public string? BreakPerDay { get; set; }

        public bool WorksSaturday { get; set; }
        public bool RotatingWeekends { get; set; }
        public int SaturdaysPerMonthRequired { get; set; }

        public decimal SaturdayHours { get; set; }

        public bool WorksSunday { get; set; }
        public decimal SundayHours { get; set; }

        public int GraceMinutesBefore { get; set; }
        public int GraceMinutesAfter { get; set; }
        public bool AllowOvertime { get; set; }
    }
}
