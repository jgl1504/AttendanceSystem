using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebApp.Shared.Model;
using WebApp.Shared.Model.Payroll;
using ClosedXML.Excel;

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

        // EXISTING: driver overtime summary
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

        // Excel export for payroll time
        [HttpGet("payroll-time/export")]
        public async Task<IActionResult> ExportPayrollTime(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? companyId,
            [FromQuery] int? departmentId,
            [FromQuery] int? employeeId)
        {
            var data = await _reportService.GetPayrollTimeSummaryAsync(
                startDate, endDate, companyId, departmentId, employeeId);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Payroll Time");

            // Header row
            ws.Cell(1, 1).Value = "Employee";
            ws.Cell(1, 2).Value = "NormalHours";
            ws.Cell(1, 3).Value = "OTWeekdayApproved";
            ws.Cell(1, 4).Value = "OTSundayPublicApproved";
            ws.Cell(1, 5).Value = "DriverApproved";

            var rowIndex = 2;
            foreach (var row in data)
            {
                ws.Cell(rowIndex, 1).Value = row.EmployeeName;
                ws.Cell(rowIndex, 2).Value = row.NormalHours;
                ws.Cell(rowIndex, 3).Value = row.OvertimeWeekdayApproved;
                ws.Cell(rowIndex, 4).Value = row.OvertimeSundayApproved;
                ws.Cell(rowIndex, 5).Value = row.DriverApproved;
                rowIndex++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            var fileName = $"PayrollTime_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";

            return File(content, contentType, fileName);
        }
    }
}
