using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Employees;
using WebApp.Server.Services.Leave;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize] // TEMP: disabled to match AttendanceController
public class LeaveController : ControllerBase
{
    private readonly LeaveService _leaveService;
    private readonly EmployeeService _employeeService;

    public LeaveController(LeaveService leaveService, EmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    // GET api/leave/balance/5
    [HttpGet("balance/{employeeId:int}")]
    public async Task<ActionResult<EmployeeLeaveDto>> GetBalance(int employeeId)
    {
        var balance = await _leaveService.GetLeaveBalanceAsync(employeeId);
        if (balance == null) return NotFound();
        return Ok(balance);
    }

    // GET api/leave/employee/5
    [HttpGet("employee/{employeeId:int}")]
    public async Task<ActionResult<List<LeaveRecordDto>>> GetEmployeeLeave(int employeeId)
    {
        var records = await _leaveService.GetLeaveRecordsForEmployeeAsync(employeeId);
        return Ok(records);
    }

    // GET api/leave/pending
    [HttpGet("pending")]
    public async Task<ActionResult<List<LeaveRecordDto>>> GetPending()
    {
        var records = await _leaveService.GetPendingLeaveRequestsAsync();
        return Ok(records);
    }

    // POST api/leave/request  (multipart/form-data: RequestLeaveDto fields + optional Attachment)
    // POST api/leave/request  (JSON only, no attachment)
    [HttpPost("request")]
    public async Task<ActionResult> RequestLeave([FromBody] RequestLeaveDto request)
    {
        var ok = await _leaveService.RequestLeaveAsync(request);
        if (!ok) return BadRequest(new { message = "Insufficient balance or invalid request." });
        return Ok();
    }


    // POST api/leave/approve/10
    [HttpPost("approve/{id:int}")]
    public async Task<ActionResult> Approve(int id)
    {
        var ok = await _leaveService.ApproveLeaveAsync(id);
        if (!ok) return BadRequest(new { message = "Unable to approve request." });
        return Ok();
    }

    // POST api/leave/reject/10
    [HttpPost("reject/{id:int}")]
    public async Task<ActionResult> Reject(int id)
    {
        var ok = await _leaveService.RejectLeaveAsync(id);
        if (!ok) return BadRequest(new { message = "Unable to reject request." });
        return Ok();
    }

    // GET api/leave/attachment-url/10
    // Returns a relative URL like /leave-attachments/{fileName} for the admin "View" button.
    [HttpGet("attachment-url/{id:int}")]
    public async Task<ActionResult<string>> GetAttachmentUrl(int id)
    {
        var fileName = await _leaveService.GetAttachmentFileNameAsync(id);
        if (string.IsNullOrWhiteSpace(fileName))
            return NotFound();

        var url = $"/leave-attachments/{fileName}";
        return Ok(url);
    }

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
