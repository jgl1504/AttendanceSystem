using Microsoft.AspNetCore.Mvc;
using WebApp.Shared.Model;
using WebApp.Server.Services.Leave;

[ApiController]
[Route("api/[controller]")]
public class LeaveBalancesController : ControllerBase
{
    private readonly LeaveBalanceService _service;

    public LeaveBalancesController(LeaveBalanceService service)
    {
        _service = service;
    }

    // GET: api/leavebalances?employeeId=123
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveBalanceEditDto>>> Get([FromQuery] int? employeeId = null)
    {
        var result = await _service.GetLeaveBalancesAsync(employeeId);
        return Ok(result);
    }

    // PUT: api/leavebalances/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Put(Guid id, LeaveBalanceEditDto dto)
    {
        var ok = await _service.UpdateLeaveBalanceAsync(id, dto);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("init")]
    public async Task<ActionResult<int>> Initialize()
    {
        // Optional: pass a specific go-live date if you want
        var createdCount = await _service.InitializeMissingBalancesAsync();

        return Ok(createdCount);
    }

}
