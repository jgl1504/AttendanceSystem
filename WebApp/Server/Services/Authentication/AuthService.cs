using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebApp.Server.Data;
using WebApp.Shared.Model;

namespace WebApp.Server.Services;

public class AuthService
{
    private readonly DataContext _context;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        DataContext context,
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public int GetEmployeeId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier));

    public string GetEmployeeEmail() =>
        _httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.Name)!;

    public async Task<ServiceResponse<string>> Login(string email, string password)
    {
        var response = new ServiceResponse<string>();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower() && e.IsActive);

        if (employee is null)
        {
            response.Success = false;
            response.Message = "Employee not found.";
            return response;
        }

        if (employee.PasswordHash == null || employee.PasswordHash.Length == 0 ||
            employee.PasswordSalt == null || employee.PasswordSalt.Length == 0)
        {
            response.Success = false;
            response.Message = "No password set for this employee.";
            return response;
        }

        if (!VerifyPasswordHash(password, employee.PasswordHash, employee.PasswordSalt))
        {
            response.Success = false;
            response.Message = "Wrong password.";
            return response;
        }

        response.Data = CreateToken(employee);
        return response;
    }

    public async Task<ServiceResponse<int>> RegisterEmployeeAsync(Employee employee, string password)
    {
        var response = new ServiceResponse<int>();

        if (await _context.Employees.AnyAsync(e => e.Email.ToLower() == employee.Email.ToLower()))
        {
            response.Success = false;
            response.Message = "Email already in use.";
            return response;
        }

        CreatePasswordHash(password, out var hash, out var salt);

        employee.PasswordHash = hash;
        employee.PasswordSalt = salt;

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        response.Data = employee.Id;
        response.Message = "Employee registered.";
        return response;
    }

    public async Task<ServiceResponse<bool>> ChangePasswordAsync(int employeeId, string newPassword)
    {
        var response = new ServiceResponse<bool>();

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee is null)
        {
            response.Success = false;
            response.Message = "Employee not found.";
            return response;
        }

        CreatePasswordHash(newPassword, out var hash, out var salt);
        employee.PasswordHash = hash;
        employee.PasswordSalt = salt;

        await _context.SaveChangesAsync();

        response.Data = true;
        response.Message = "Password changed.";
        return response;
    }

    private string CreateToken(Employee employee)
    {
        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
        new(ClaimTypes.Name, employee.Email),
        new(ClaimTypes.Role, employee.Role), // "Admin" or "Employee"
        new("employee_id", employee.Id.ToString()) // Explicit employee ID claim
    };

        var jwtSection = _configuration.GetSection("Jwt");
        var key = jwtSection["Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }

    public async Task<Employee?> FindEmployeeByEmailAsync(string email)
    {
        return await _context.Employees
            .FirstOrDefaultAsync(e => e.Email.ToLower() == email.ToLower());
    }

    public async Task<ServiceResponse<int>> SetInitialPasswordAsync(Employee employee, string password)
    {
        var response = new ServiceResponse<int>();

        // Ensure no password exists yet
        if (employee.PasswordHash != null && employee.PasswordHash.Length > 0 &&
            employee.PasswordSalt != null && employee.PasswordSalt.Length > 0)
        {
            response.Success = false;
            response.Message = "Password already set. Please log in or use change password.";
            return response;
        }

        CreatePasswordHash(password, out var hash, out var salt);
        employee.PasswordHash = hash;
        employee.PasswordSalt = salt;

        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();

        response.Data = employee.Id;
        response.Message = "Password set.";
        return response;
    }

}
