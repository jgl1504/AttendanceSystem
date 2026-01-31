using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Attendance;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SitesController : ControllerBase
{
    private readonly SiteService _service;

    public SitesController(SiteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Site>>> GetAll()
        => await _service.GetAllAsync();

    [HttpGet("active")]
    public async Task<ActionResult<List<Site>>> GetActive()
        => await _service.GetActiveAsync();

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Site>> GetById(Guid id)
    {
        var site = await _service.GetByIdAsync(id);
        if (site is null) return NotFound();
        return site;
    }

    [HttpPost]
    public async Task<ActionResult<Site>> Create(Site site)
    {
        var created = await _service.CreateAsync(site);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, Site site)
    {
        if (id != site.Id) return BadRequest();

        var ok = await _service.UpdateAsync(site);
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
