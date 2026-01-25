using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Attendance;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize] // temporarily disabled - UI controls access
public class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("status/{employeeId:int}")]
    public async Task<ActionResult<ClockStatusDto>> GetStatus(int employeeId)
    {
        var status = await _attendanceService.GetStatusAsync(employeeId);
        return Ok(status);
    }

    [HttpPost("clockin")]
    public async Task<IActionResult> ClockIn([FromBody] ClockRequestDto request)
    {
        // TEMP: use selected employee as the clocking user as well
        var ok = await _attendanceService.ClockInAsync(request, request.EmployeeId);
        if (!ok)
            return BadRequest(new { message = "Employee is already clocked in." });

        return Ok();
    }

    [HttpPost("clockout")]
    public async Task<IActionResult> ClockOut([FromBody] ClockRequestDto request)
    {
        // TEMP: use selected employee as the clocking user as well
        var ok = await _attendanceService.ClockOutAsync(request, request.EmployeeId);
        if (!ok)
            return BadRequest(new { message = "Employee is not currently clocked in." });

        return Ok();
    }

    [HttpGet("today")]
    public async Task<ActionResult<IEnumerable<AttendanceListItemDto>>> GetToday()
    {
        var records = await _attendanceService.GetTodayAsync();
        return Ok(records);
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<AttendanceListItemDto>>> GetList(
        [FromQuery] DateTime date,
        [FromQuery] int? employeeId)
    {
        var localDay = date.Date;
        var records = await _attendanceService.GetByDateAsync(localDay, employeeId);
        return Ok(records);
    }

    [HttpPut("update-times")]
    public async Task<IActionResult> UpdateTimes([FromBody] AttendanceEditDto dto)
    {
        var ok = await _attendanceService.UpdateTimesAsync(dto);
        if (!ok)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _attendanceService.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    // Unified overtime decision DTO (approve/deny + location/note + approver)
    public class OvertimeDecisionDto
    {
        public int Id { get; set; }                 // AttendanceRecord.Id
        public OvertimeStatus OvertimeStatus { get; set; }
        public string? OvertimeLocation { get; set; }
        public string? OvertimeNote { get; set; }

        // Chosen approver employee id from the dropdown (nullable for reopen)
        public int? OvertimeApprovedByEmployeeId { get; set; }
    }

    [HttpPost("overtime-decision")]
    public async Task<IActionResult> DecideOvertime([FromBody] OvertimeDecisionDto dto)
    {
        var ok = await DecideOvertimeInternalAsync(
            dto.Id,
            dto.OvertimeStatus,
            dto.OvertimeLocation,
            dto.OvertimeNote,
            dto.OvertimeApprovedByEmployeeId); // pass nullable

        if (!ok) return NotFound();
        return NoContent();
    }

    // Internal helper that uses the AttendanceService + current approver
    private async Task<bool> DecideOvertimeInternalAsync(
        int attendanceId,
        OvertimeStatus status,
        string? location,
        string? note,
        int? approverEmployeeId)
    {
        var record = await _attendanceService.GetRecordForDecisionAsync(attendanceId);
        if (record is null) return false;

        if ((record.HoursWorked ?? 0) <= 0 || record.ClockOutTime is null)
            return false;

        record.OvertimeStatus = status;
        record.OvertimeLocation = location;
        record.OvertimeNote = note;

        if (status == OvertimeStatus.Approved || status == OvertimeStatus.Denied)
        {
            // Must have a valid approver to satisfy FK
            if (approverEmployeeId is null || approverEmployeeId <= 0)
                return false;

            record.OvertimeApprovedByEmployeeId = approverEmployeeId;
            record.OvertimeDecisionTime = DateTime.UtcNow;
        }
        else
        {
            // Pending / None (reopen) – clear approver + decision time
            record.OvertimeApprovedByEmployeeId = null;
            record.OvertimeDecisionTime = null;
        }

        await _attendanceService.SaveChangesAsync();
        return true;
    }

    // Helper kept for when you wire real user->employee mapping
    private async Task<int?> GetCurrentEmployeeIdAsync()
    {
        var employeeIdClaim = User.FindFirst("employee_id") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out var id))
            return id;

        return await Task.FromResult<int?>(null);
    }

    [HttpGet("quick-entry-data")]
    public async Task<ActionResult<List<QuickEntryRowDto>>> GetQuickEntryData([FromQuery] DateTime date)
    {
        var rows = await _attendanceService.GetQuickEntryDataAsync(date);
        return Ok(rows);
    }

    [HttpPost("save-quick-entry")]
    public async Task<IActionResult> SaveQuickEntry([FromBody] SaveQuickEntryDto dto)
    {
        // TEMP: use admin employee ID = 1
        var clockedBy = 1;

        var ok = await _attendanceService.SaveQuickEntryAsync(
            dto.EmployeeId,
            dto.Date,
            dto.ClockInTime,
            dto.ClockOutTime,
            clockedBy /* category currently set to Normal inside service */);

        if (!ok)
            return BadRequest(new { message = "Failed to save entry." });

        return Ok();
    }

    [HttpPost("save-quick-entry-batch")]
    public async Task<IActionResult> SaveQuickEntryBatch([FromBody] SaveQuickEntryBatchDto dto)
    {
        // Prevent saving today or future dates via Quick Entry
        if (dto.Date.Date >= DateTime.Today)
        {
            return BadRequest(new { message = "Quick Entry can only be used for past dates." });
        }

        var clockedBy = await GetCurrentEmployeeIdAsync();

        if (clockedBy == null)
        {
            clockedBy = dto.Entries.FirstOrDefault()?.EmployeeId ?? 0;
        }

        if (clockedBy == 0)
            return BadRequest(new { message = "Could not determine who is clocking." });

        try
        {
            foreach (var entry in dto.Entries)
            {
                var ok = await _attendanceService.SaveQuickEntryAsync(
                    entry.EmployeeId,
                    dto.Date,
                    entry.ClockInTime,
                    entry.ClockOutTime,
                    clockedBy.Value /* category currently Normal in service */);

                if (!ok)
                {
                    return BadRequest(new { message = $"Failed to save entry for employee {entry.EmployeeId}" });
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("clear-today/{employeeId:int}")]
    public async Task<IActionResult> ClearTodayRecord(int employeeId)
    {
        var ok = await _attendanceService.ClearTodayRecordAsync(employeeId);
        if (!ok)
            return NotFound(new { message = "No record found for today." });

        return Ok(new { message = "Today's record(s) cleared." });
    }
}
