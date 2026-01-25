using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Departments;

public class DepartmentService
{
    private readonly DataContext _context;

    public DepartmentService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Department>> GetAllAsync()
        => await _context.Departments
            .OrderBy(d => d.Name)
            .AsNoTracking()
            .ToListAsync();

    public async Task<Department?> GetByIdAsync(int id)
        => await _context.Departments.FindAsync(id);

    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department?> UpdateAsync(int id, Department department)
    {
        var existing = await _context.Departments.FindAsync(id);
        if (existing is null) return null;

        existing.Name = department.Name;

        // Work rules
        existing.RequiredHoursPerWeek = department.RequiredHoursPerWeek;
        existing.DailyStartTime = department.DailyStartTime;
        existing.DailyEndTime = department.DailyEndTime;
        existing.BreakPerDay = department.BreakPerDay;

        existing.WorksSaturday = department.WorksSaturday;
        existing.RotatingWeekends = department.RotatingWeekends;
        existing.SaturdaysPerMonthRequired = department.SaturdaysPerMonthRequired;

        // Weekend hours (new)
        existing.SaturdayHours = department.SaturdayHours;
        existing.WorksSunday = department.WorksSunday;
        existing.SundayHours = department.SundayHours;

        // Grace / overtime
        existing.GraceMinutesBefore = department.GraceMinutesBefore;
        existing.GraceMinutesAfter = department.GraceMinutesAfter;
        existing.AllowOvertime = department.AllowOvertime;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Departments.FindAsync(id);
        if (existing is null) return false;

        _context.Departments.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
