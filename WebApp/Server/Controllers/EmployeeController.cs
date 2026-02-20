using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Employees;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EmployeeService _service;

    public EmployeesController(EmployeeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _service.GetAllAsync();

        var items = employees
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                Phone = e.Phone,
                HireDate = e.HireDate,
                IsActive = e.IsActive,

                CompanyId = e.CompanyId,
                Company = e.Company == null
                    ? null
                    : new CompanyDto
                    {
                        Id = e.Company.Id,
                        Name = e.Company.Name
                    },

                DepartmentId = e.DepartmentId,
                Department = e.Department == null
                    ? null
                    : new DepartmentDto
                    {
                        Id = e.Department.Id,
                        Name = e.Department.Name
                    },

                Role = e.Role
            })
            .ToList();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _service.GetByIdAsync(id);
        if (employee is null) return NotFound();

        var dto = new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Phone = employee.Phone,
            HireDate = employee.HireDate,
            IsActive = employee.IsActive,

            CompanyId = employee.CompanyId,
            Company = employee.Company == null
                ? null
                : new CompanyDto
                {
                    Id = employee.Company.Id,
                    Name = employee.Company.Name
                },

            DepartmentId = employee.DepartmentId,
            Department = employee.Department == null
                ? null
                : new DepartmentDto
                {
                    Id = employee.Department.Id,
                    Name = employee.Department.Name
                },

            Role = employee.Role
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] EmployeeDto dto)
    {
        // map DTO -> entity
        var employee = new Employee
        {
            Name = dto.Name,
            Email = dto.Email,
            Phone = dto.Phone,
            HireDate = dto.HireDate,
            IsActive = dto.IsActive,
            CompanyId = dto.CompanyId,
            DepartmentId = dto.DepartmentId,
            Role = dto.Role
        };

        var created = await _service.CreateAsync(employee);

        // map back to DTO (id + keep FKs)
        dto.Id = created.Id;

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        // Map incoming DTO onto existing entity
        existing.Name = dto.Name;
        existing.Email = dto.Email;
        existing.Phone = dto.Phone;
        existing.HireDate = dto.HireDate;
        existing.IsActive = dto.IsActive;
        existing.CompanyId = dto.CompanyId;
        existing.DepartmentId = dto.DepartmentId;
        existing.Role = dto.Role;

        await _service.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    [HttpPost("{id:int}/role")]
    public async Task<IActionResult> SetRole(int id, [FromBody] SetRoleRequest request)
    {
        var ok = await _service.SetEmployeeRoleAsync(id, request.RoleName);
        if (!ok) return NotFound();
        return NoContent();
    }

    // reset custom employee password so they can do first-time login again
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var ok = await _service.ClearEmployeePasswordAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    public class SetRoleRequest
    {
        public string RoleName { get; set; } = string.Empty; // "Admin" or "Employee"
    }
}
