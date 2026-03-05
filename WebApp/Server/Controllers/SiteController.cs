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

    // DELETE with "in use" handling
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // Let the service tell us what happened
        var result = await _service.DeleteAsync(id);

        // Suggestion for service return:
        // 0 = not found, 1 = deleted, 2 = in use / cannot delete
        switch (result)
        {
            case 1:
                return NoContent();

            case 2:
                // 409 Conflict is a good fit for "cannot delete due to existing links"
                return Conflict("Cannot delete this site because it is linked to other records.");

            default:
                return NotFound();
        }
    }
}
