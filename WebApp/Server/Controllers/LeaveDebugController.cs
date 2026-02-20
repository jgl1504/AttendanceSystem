using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services;
using WebApp.Shared.Model.Payroll;

namespace WebApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveDebugController : ControllerBase
    {
        private readonly LeaveDebugService _leaveDebugService;

        public LeaveDebugController(LeaveDebugService leaveDebugService)
        {
            _leaveDebugService = leaveDebugService;
        }

        // DEBUG: Per-type lines for a single employee + month
        [HttpGet("employee-lines")]
        public async Task<ActionResult<List<EmployeeLeaveLineDto>>> GetEmployeeLeaveLines(
            int employeeId,
            int year,
            int month)
        {
            var data = await _leaveDebugService.GetEmployeeLeaveLinesForMonthAsync(employeeId, year, month);
            return Ok(data);
        }

        // DEBUG: Per-type balances for all employees for a month
        [HttpGet("type-balances")]
        public async Task<ActionResult<List<LeaveTypeBalanceRowDto>>> GetLeaveTypeBalances(
            int year,
            int month)
        {
            var data = await _leaveDebugService.GetLeaveTypeBalancesForMonthAsync(year, month);
            return Ok(data);
        }

        // In your LeaveDebugController
        [HttpGet("single-summary")]
        public async Task<IActionResult> GetSingleSummary(
            int employeeId,
            Guid leaveTypeId,
            DateTime asAtDate,
            [FromServices] LeaveDebugService debugService)
        {
            var summary = await debugService.GetLeaveSummaryAsync(employeeId, leaveTypeId, asAtDate.Date);
            return Ok(summary);
        }

    }
}
