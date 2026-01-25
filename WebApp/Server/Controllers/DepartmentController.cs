using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services.Departments;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _service;

    public DepartmentsController(DepartmentService service)
    {
        _service = service;
    }

    // GET api/departments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
    {
        var entities = await _service.GetAllAsync();

        var result = entities.Select(d => new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            RequiredHoursPerWeek = d.RequiredHoursPerWeek,
            DailyStartTime = d.DailyStartTime.ToString(@"hh\:mm"),
            DailyEndTime = d.DailyEndTime.ToString(@"hh\:mm"),
            BreakPerDay = d.BreakPerDay.ToString(@"hh\:mm"),
            WorksSaturday = d.WorksSaturday,
            RotatingWeekends = d.RotatingWeekends,
            SaturdaysPerMonthRequired = d.SaturdaysPerMonthRequired,
            SaturdayHours = d.SaturdayHours,
            WorksSunday = d.WorksSunday,
            SundayHours = d.SundayHours,
            GraceMinutesBefore = d.GraceMinutesBefore,
            GraceMinutesAfter = d.GraceMinutesAfter,
            AllowOvertime = d.AllowOvertime
        });

        return Ok(result);
    }

    // GET api/departments/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var d = await _service.GetByIdAsync(id);
        if (d is null) return NotFound();

        var dto = new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            RequiredHoursPerWeek = d.RequiredHoursPerWeek,
            DailyStartTime = d.DailyStartTime.ToString(@"hh\:mm"),
            DailyEndTime = d.DailyEndTime.ToString(@"hh\:mm"),
            BreakPerDay = d.BreakPerDay.ToString(@"hh\:mm"),
            WorksSaturday = d.WorksSaturday,
            RotatingWeekends = d.RotatingWeekends,
            SaturdaysPerMonthRequired = d.SaturdaysPerMonthRequired,
            SaturdayHours = d.SaturdayHours,
            WorksSunday = d.WorksSunday,
            SundayHours = d.SundayHours,
            GraceMinutesBefore = d.GraceMinutesBefore,
            GraceMinutesAfter = d.GraceMinutesAfter,
            AllowOvertime = d.AllowOvertime
        };

        return Ok(dto);
    }

    // POST api/departments
    [HttpPost]
    public async Task<ActionResult<DepartmentDto>> Create([FromBody] DepartmentDto dto)
    {
        // Name
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            ModelState.AddModelError(nameof(dto.Name), "Department name is required.");
        }

        // Times with validation
        var start = ParseAndValidateTime(dto.DailyStartTime, nameof(dto.DailyStartTime));
        var end = ParseAndValidateTime(dto.DailyEndTime, nameof(dto.DailyEndTime));
        var brk = ParseAndValidateTime(dto.BreakPerDay, nameof(dto.BreakPerDay));

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var entity = new Department
        {
            Name = dto.Name,
            RequiredHoursPerWeek = dto.RequiredHoursPerWeek,
            DailyStartTime = start,
            DailyEndTime = end,
            BreakPerDay = brk,
            WorksSaturday = dto.WorksSaturday,
            RotatingWeekends = dto.RotatingWeekends,
            SaturdaysPerMonthRequired = dto.SaturdaysPerMonthRequired,
            SaturdayHours = dto.SaturdayHours,
            WorksSunday = dto.WorksSunday,
            SundayHours = dto.SundayHours,
            GraceMinutesBefore = dto.GraceMinutesBefore,
            GraceMinutesAfter = dto.GraceMinutesAfter,
            AllowOvertime = dto.AllowOvertime
        };

        var created = await _service.CreateAsync(entity);

        dto.Id = created.Id;
        dto.DailyStartTime = created.DailyStartTime.ToString(@"hh\:mm");
        dto.DailyEndTime = created.DailyEndTime.ToString(@"hh\:mm");
        dto.BreakPerDay = created.BreakPerDay.ToString(@"hh\:mm");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, dto);
    }

    // PUT api/departments/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] DepartmentDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var existing = await _service.GetByIdAsync(id);
        if (existing is null) return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            ModelState.AddModelError(nameof(dto.Name), "Department name is required.");
        }

        // Only validate times that were sent; keep existing if null/empty
        TimeSpan? start = null, end = null, brk = null;

        if (!string.IsNullOrWhiteSpace(dto.DailyStartTime))
            start = ParseAndValidateTime(dto.DailyStartTime, nameof(dto.DailyStartTime));

        if (!string.IsNullOrWhiteSpace(dto.DailyEndTime))
            end = ParseAndValidateTime(dto.DailyEndTime, nameof(dto.DailyEndTime));

        if (!string.IsNullOrWhiteSpace(dto.BreakPerDay))
            brk = ParseAndValidateTime(dto.BreakPerDay, nameof(dto.BreakPerDay));

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        existing.Name = dto.Name;
        existing.RequiredHoursPerWeek = dto.RequiredHoursPerWeek;
        if (start.HasValue) existing.DailyStartTime = start.Value;
        if (end.HasValue) existing.DailyEndTime = end.Value;
        if (brk.HasValue) existing.BreakPerDay = brk.Value;

        existing.WorksSaturday = dto.WorksSaturday;
        existing.RotatingWeekends = dto.RotatingWeekends;
        existing.SaturdaysPerMonthRequired = dto.SaturdaysPerMonthRequired;
        existing.SaturdayHours = dto.SaturdayHours;
        existing.WorksSunday = dto.WorksSunday;
        existing.SundayHours = dto.SundayHours;
        existing.GraceMinutesBefore = dto.GraceMinutesBefore;
        existing.GraceMinutesAfter = dto.GraceMinutesAfter;
        existing.AllowOvertime = dto.AllowOvertime;

        await _service.UpdateAsync(id, existing);
        return NoContent();
    }

    // DELETE api/departments/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }

    private TimeSpan ParseAndValidateTime(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ModelState.AddModelError(fieldName, "This time is required (HH:mm).");
            return default;
        }

        if (!TimeSpan.TryParse(value, out var ts))
        {
            ModelState.AddModelError(fieldName, "Invalid time format. Use HH:mm (e.g. 08:00).");
            return default;
        }

        if (ts >= new TimeSpan(24, 0, 0))
        {
            ModelState.AddModelError(fieldName, "Time must be less than 24:00.");
            return default;
        }

        return ts;
    }
}
