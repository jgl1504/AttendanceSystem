using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Shared.Model.Constants
{
    public static class LeaveConstants
    {
        // From this date onward, use Employee.StartDate as the accrual start
        public static readonly DateTime AccrualHireDateCutOff =
            new DateTime(2026, 1, 1);
    }

}
