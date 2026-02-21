using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WebApp.Server.Services.Leave;
using WebApp.Shared.Model;
using WebApp.Shared.Model.Constants;
using WebApp.Shared.Model.Payroll;
using ClosedXML.Excel;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize] // TEMP: disabled to match AttendanceController
public class LeaveController : ControllerBase
{
    private readonly LeaveService _leaveService;
    private readonly LeaveRequestService _leaveRequestService;

    public LeaveController(LeaveService leaveService, LeaveRequestService leaveRequestService)
    {
        _leaveService = leaveService;
        _leaveRequestService = leaveRequestService;
    }

    // ===== NEW BALANCE / SUMMARY (per leave type) =====

    // GET api/leave/balance-summary/{employeeId}/{leaveTypeId}?asAtDate=2026-03-31
    [HttpGet("balance-summary/{employeeId:int}/{leaveTypeId:guid}")]
    public async Task<ActionResult<LeaveBalanceSummaryDto>> GetBalanceSummary(
        int employeeId,
        Guid leaveTypeId,
        [FromQuery] DateTime? asAtDate = null)
    {
        var date = (asAtDate ?? DateTime.Today).Date;
        var summary = await _leaveService.GetLeaveSummaryAsync(employeeId, leaveTypeId, date);
        if (summary == null) return NotFound();
        return Ok(summary);
    }

    // GET api/leave/employee-balance-lines/{employeeId}?year=2026&month=3
    [HttpGet("employee-balance-lines/{employeeId:int}")]
    public async Task<ActionResult<List<EmployeeLeaveLineDto>>> GetEmployeeBalanceLines(
        int employeeId,
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (year <= 0 || month < 1 || month > 12)
            return BadRequest(new { message = "Invalid year or month." });

        var data = await _leaveService.GetEmployeeLeaveLinesForMonthAsync(employeeId, year, month);
        return Ok(data);
    }

    // GET api/leave/type-balances?year=2026&month=3
    [HttpGet("type-balances")]
    public async Task<ActionResult<List<LeaveTypeBalanceRowDto>>> GetLeaveTypeBalances(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        if (year <= 0 || month < 1 || month > 12)
            return BadRequest(new { message = "Invalid year or month." });

        var data = await _leaveService.GetLeaveTypeBalancesForMonthAsync(year, month);
        return Ok(data);
    }

    // ===== REQUESTS & HISTORY (LeaveRequestService) =====

    // GET api/leave/employee/5
    [HttpGet("employee/{employeeId:int}")]
    public async Task<ActionResult<List<LeaveRecordDto>>> GetEmployeeLeave(int employeeId)
    {
        var records = await _leaveRequestService.GetLeaveRecordsForEmployeeAsync(employeeId);
        return Ok(records);
    }

    // GET api/leave/pending
    [HttpGet("pending")]
    public async Task<ActionResult<List<LeaveRecordDto>>> GetPending()
    {
        var records = await _leaveRequestService.GetPendingLeaveRequestsAsync();
        return Ok(records);
    }

    // POST api/leave/request  (JSON only, no attachment for now)
    [HttpPost("request")]
    public async Task<ActionResult> RequestLeave([FromBody] RequestLeaveDto request)
    {
        var ok = await _leaveRequestService.RequestLeaveAsync(request);
        if (!ok) return BadRequest(new { message = "Insufficient balance or invalid request." });
        return Ok();
    }

    // POST api/leave/{id}/attachment  (multipart/form-data)
    [HttpPost("{id:int}/attachment")]
    public async Task<ActionResult> UploadAttachment(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var ok = await _leaveRequestService.SaveAttachmentAsync(id, file);
        if (!ok) return BadRequest(new { message = "Unable to save attachment." });

        return Ok();
    }

    // POST api/leave/approve/10
    [HttpPost("approve/{id:int}")]
    public async Task<ActionResult> Approve(int id)
    {
        var ok = await _leaveRequestService.ApproveLeaveAsync(id);
        if (!ok) return BadRequest(new { message = "Unable to approve request." });
        return Ok();
    }

    // POST api/leave/reject/10
    [HttpPost("reject/{id:int}")]
    public async Task<ActionResult> Reject(int id)
    {
        var ok = await _leaveRequestService.RejectLeaveAsync(id);
        if (!ok) return BadRequest(new { message = "Unable to reject request." });
        return Ok();
    }

    // GET api/leave/attachment-url/10
    [HttpGet("attachment-url/{id:int}")]
    public async Task<ActionResult<string>> GetAttachmentUrl(int id)
    {
        var fileName = await _leaveRequestService.GetAttachmentFileNameAsync(id);
        if (string.IsNullOrWhiteSpace(fileName))
            return NotFound();

        var url = $"/leave-attachments/{fileName}";
        return Ok(url);
    }

    // POST api/leave/change-type/10
    [HttpPost("change-type/{id:int}")]
    public async Task<ActionResult> ChangeType(int id, [FromBody] Guid newLeaveTypeId)
    {
        var ok = await _leaveRequestService.ChangeLeaveTypeAsync(id, newLeaveTypeId);
        if (!ok) return BadRequest(new { message = "Unable to change leave type." });
        return Ok();
    }

    // GET api/leave/records?from=2026-02-01&to=2026-02-28&employeeId=5&departmentId=1
    [HttpGet("records")]
    public async Task<ActionResult<List<PayrollLeaveDetailDto>>> GetLeaveRecords(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? departmentId = null)
    {
        var records = await _leaveRequestService.GetLeaveRecordsForPeriodAsync(from, to, employeeId, departmentId);
        return Ok(records);
    }

    // GET api/leave/payroll-matrix?year=2026&month=3&companyId=1&departmentId=2&employeeId=5
    [HttpGet("payroll-matrix")]
    public async Task<ActionResult<List<PayrollLeaveRowDto>>> GetPayrollMatrix(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? companyId = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? employeeId = null)
    {
        if (year <= 0 || month < 1 || month > 12)
            return BadRequest(new { message = "Invalid year or month." });

        var data = await _leaveService.GetPayrollLeaveMatrixAsync(
            year,
            month,
            companyId,
            departmentId,
            employeeId);

        return Ok(data);
    }

    // Excel export using the SAME data and filters as payroll-matrix
    // GET api/leave/payroll-matrix/export?year=2026&month=3&companyId=1&departmentId=2&employeeId=5
    [HttpGet("payroll-matrix/export")]
    public async Task<IActionResult> ExportPayrollMatrix(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? companyId = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? employeeId = null)
    {
        if (year <= 0 || month < 1 || month > 12)
            return BadRequest(new { message = "Invalid year or month." });

        var data = await _leaveService.GetPayrollLeaveMatrixAsync(
            year,
            month,
            companyId,
            departmentId,
            employeeId);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Payroll Leave");

        // Header
        ws.Cell(1, 1).Value = "Employee";
        ws.Cell(1, 2).Value = "AnnualLeave";
        ws.Cell(1, 3).Value = "PaternityLeave";
        ws.Cell(1, 4).Value = "MaternityLeave";
        ws.Cell(1, 5).Value = "SickLeave";
        ws.Cell(1, 6).Value = "UnpaidLeave";
        ws.Cell(1, 7).Value = "FamilyResponsibility";

        var rowIndex = 2;
        foreach (var row in data)
        {
            ws.Cell(rowIndex, 1).Value = row.EmployeeName;
            ws.Cell(rowIndex, 2).Value = row.AnnualLeave;
            ws.Cell(rowIndex, 3).Value = row.PaternityLeave;
            ws.Cell(rowIndex, 4).Value = row.MaternityLeave;
            ws.Cell(rowIndex, 5).Value = row.SickLeave;
            ws.Cell(rowIndex, 6).Value = row.UnpaidLeave;
            ws.Cell(rowIndex, 7).Value = row.FamilyResponsibility;
            rowIndex++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        var fileName = $"PayrollLeave_{year}_{month:00}.xlsx";

        return File(content, contentType, fileName);
    }
}
