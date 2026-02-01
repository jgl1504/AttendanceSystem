using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Shared.Model
{
    public class LeaveBalanceEditDto
    {
        public Guid Id { get; set; }              // LeaveBalance.Id
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;

        public Guid LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; } = string.Empty;

        public DateTime BalanceStartDate { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
    }

}
