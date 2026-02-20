using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public CompanyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Company>>> GetAll()
        {
            var companies = await _companyService.GetAllAsync();
            return Ok(companies);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Company>> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<ActionResult<Company>> Create([FromBody] Company company)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var created = await _companyService.CreateAsync(company);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Company>> Update(int id, [FromBody] Company company)
        {
            if (id != company.Id)
                return BadRequest("Id mismatch.");

            var updated = await _companyService.UpdateAsync(id, company);
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _companyService.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
    }
}
