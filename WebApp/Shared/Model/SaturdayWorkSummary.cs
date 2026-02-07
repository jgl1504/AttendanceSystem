using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Shared.Model
{
    public class SaturdayWorkSummary
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;

        public int ExpectedSaturdays { get; set; }   // should work
        public int WorkedSaturdays { get; set; }     // have attendance
        public int MissedSaturdays { get; set; }     // Expected - Worked
    }

}
