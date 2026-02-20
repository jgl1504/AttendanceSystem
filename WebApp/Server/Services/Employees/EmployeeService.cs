using Microsoft.EntityFrameworkCore;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services.Employees;

public class EmployeeService
{
    private readonly DataContext _context;

    public EmployeeService(DataContext context)
    {
        _context = context;
    }

    public async Task<List<Employee>> GetAllAsync()
    {
        // Load both Company and Department, no back-references
        return await _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Department)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(int id)
        => await _context.Employees
            .Include(e => e.Company)
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Employee> CreateAsync(Employee employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    // helper used by the controller after mapping DTO -> entity
    public Task<int> SaveChangesAsync()
        => _context.SaveChangesAsync();

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Employees.FindAsync(id);
        if (existing is null) return false;

        _context.Employees.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetEmployeeRoleAsync(int employeeId, string roleName)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null) return false;

        // normalize and validate
        if (roleName != "Admin" && roleName != "Employee")
            return false;

        employee.Role = roleName;
        await _context.SaveChangesAsync();
        return true;
    }

    // clear password so employee can perform first-time login flow again
    public async Task<bool> ClearEmployeePasswordAsync(int employeeId)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null) return false;

        employee.PasswordHash = Array.Empty<byte>();
        employee.PasswordSalt = Array.Empty<byte>();

        await _context.SaveChangesAsync();
        return true;
    }
}
