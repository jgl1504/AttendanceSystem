using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Leave;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveTypesController : ControllerBase
{
    private readonly LeaveTypeService _service;

    public LeaveTypesController(LeaveTypeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<LeaveType>>> GetAll()
        => await _service.GetAllAsync();

    [HttpGet("active")]
    public async Task<ActionResult<List<LeaveType>>> GetActive()
        => await _service.GetActiveAsync();

    [HttpGet("pools")]
    public async Task<ActionResult<List<LeaveType>>> GetPoolTypes()
        => await _service.GetPoolTypesAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LeaveType>> GetById(Guid id)
    {
        var leaveType = await _service.GetByIdAsync(id);
        if (leaveType == null) return NotFound();
        return leaveType;
    }

    [HttpPost]
    public async Task<ActionResult<LeaveType>> Create(LeaveType leaveType)
    {
        var created = await _service.CreateAsync(leaveType);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, LeaveType leaveType)
    {
        if (id != leaveType.Id) return BadRequest();

        var ok = await _service.UpdateAsync(leaveType);
        if (!ok) return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();

        return NoContent();
    }
}
