using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Shared.Model;
using WebApp.Shared.Model.Payroll;

namespace WebApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportController(ReportService reportService)
        {
            _reportService = reportService;
        }

        // NEW: payroll time summary (16th–15th or any custom range you pass)
        // GET: api/report/payroll-time?startDate=2025-12-16&endDate=2026-01-16&companyId=1&departmentId=2&employeeId=3
        [HttpGet("payroll-time")]
        public async Task<ActionResult<List<PayrollTimeSummaryDto>>> GetPayrollTimeSummary(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] int? employeeId)
        {
            if (endDate <= startDate)
                return BadRequest("End date must be after start date (exclusive upper bound).");

            var result = await _reportService.GetPayrollTimeSummaryAsync(startDate, endDate, companyId, departmentId, employeeId);
            return Ok(result);
        }

        // EXISTING: driver overtime summary (you can keep if still used elsewhere)
        // GET: api/report/overtime?startDate=2025-12-16&endDate=2026-01-15&employeeId=123
        [HttpGet("overtime")]
        public async Task<ActionResult<List<DriverOvertimeSummary>>> GetOvertime(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? employeeId)
        {
            if (endDate < startDate)
                return BadRequest("End date must be on or after start date.");

            var result = await _reportService.GetDriverOvertimeSummaryAsync(startDate, endDate, employeeId);
            return Ok(result);
        }

        // EXISTING: leave summary
        // GET: api/report/leave?startDate=2025-12-16&endDate=2026-01-15&employeeId=123
        [HttpGet("leave")]
        public async Task<ActionResult<List<LeaveSummary>>> GetLeave(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? employeeId)
        {
            if (endDate < startDate)
                return BadRequest("End date must be on or after start date.");

            var result = await _reportService.GetLeaveSummaryAsync(startDate, endDate, employeeId);
            return Ok(result);
        }

        // EXISTING: Saturdays report
        // GET: api/report/saturdays?startDate=2025-12-16&endDate=2026-01-15&employeeId=123
        [HttpGet("saturdays")]
        public async Task<ActionResult<List<SaturdayWorkSummary>>> GetSaturdayReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? employeeId)
        {
            if (endDate < startDate)
                return BadRequest("End date must be on or after start date.");

            var result = await _reportService.GetSaturdayWorkReportAsync(startDate, endDate, employeeId);
            return Ok(result);
        }
    }
}
