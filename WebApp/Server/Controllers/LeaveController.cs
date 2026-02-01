using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Leave;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize] // TEMP: disabled to match AttendanceController
public class LeaveController : ControllerBase
{
    // Legacy balance + setup (EmployeeLeaves)
    private readonly LeaveService _leaveService;

    // Requests, history, approve/reject (LeaveRecords)
    private readonly LeaveRequestService _leaveRequestService;

    public LeaveController(LeaveService leaveService, LeaveRequestService leaveRequestService)
    {
        _leaveService = leaveService;
        _leaveRequestService = leaveRequestService;
    }

    // ===== BALANCE (legacy EmployeeLeaves, used by request form card) =====

    // GET api/leave/balance/5
    [HttpGet("balance/{employeeId:int}")]
    public async Task<ActionResult<EmployeeLeaveDto>> GetBalance(int employeeId)
    {
        var balance = await _leaveService.GetLeaveBalanceAsync(employeeId);
        if (balance == null) return NotFound();
        return Ok(balance);
    }

    // GET api/leave/balance-summary/5/{leaveTypeId}
    // Used by admin page to see entitlement/taken/remaining for a specific type (Annual, Maternity, etc.)
    [HttpGet("balance-summary/{employeeId:int}/{leaveTypeId:guid}")]
    public async Task<ActionResult<LeaveBalanceSummaryDto>> GetBalanceSummary(int employeeId, Guid leaveTypeId)
    {
        var summary = await _leaveService.GetLeaveBalanceSummaryAsync(employeeId, leaveTypeId);
        if (summary == null) return NotFound();
        return Ok(summary);
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
    // Returns a relative URL like /leave-attachments/{fileName} for the admin "View" button.
    [HttpGet("attachment-url/{id:int}")]
    public async Task<ActionResult<string>> GetAttachmentUrl(int id)
    {
        var fileName = await _leaveRequestService.GetAttachmentFileNameAsync(id);
        if (string.IsNullOrWhiteSpace(fileName))
            return NotFound();

        var url = $"/leave-attachments/{fileName}";
        return Ok(url);
    }

    // ===== LEGACY SETUP SCREEN (admin /admin/leave/setup) =====

    // GET api/leave/setup
    [HttpGet("setup")]
    public async Task<ActionResult<List<LeaveSetupRowDto>>> GetSetup()
    {
        var rows = await _leaveService.GetLeaveSetupAsync();
        return Ok(rows);
    }

    // POST api/leave/setup
    [HttpPost("setup")]
    public async Task<ActionResult> SaveSetup([FromBody] LeaveSetupRowDto dto)
    {
        var ok = await _leaveService.SaveLeaveSetupAsync(dto);
        if (!ok) return BadRequest(new { message = "Failed to save leave setup." });
        return Ok();
    }
}
