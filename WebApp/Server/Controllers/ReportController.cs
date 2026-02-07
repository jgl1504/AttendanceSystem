using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Shared.Model;

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
