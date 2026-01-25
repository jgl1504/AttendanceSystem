using Microsoft.AspNetCore.Mvc;
using WebApp.Server.Services;
using WebApp.Shared.Model;

namespace WebApp.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public int EmployeeId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ServiceResponse<string>>> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.Login(request.Email, request.Password);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// First-time password set for an existing employee record.
    /// Admin must have created the employee with this email first.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<ServiceResponse<int>>> Register([FromBody] RegisterRequest request)
    {
        // Look up employee by email
        var employee = await _authService.FindEmployeeByEmailAsync(request.Email);
        if (employee is null)
        {
            return BadRequest(new ServiceResponse<int>
            {
                Success = false,
                Message = "Employee not found. Ask admin to create your profile first."
            });
        }

        // Use SetInitialPasswordAsync so existing email is allowed
        var result = await _authService.SetInitialPasswordAsync(employee, request.Password);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }


    [HttpPost("reset-password")]
    // [Authorize(Roles = "Admin")]  // enable once JWT + policies are wired on the API
    public async Task<ActionResult<ServiceResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ChangePasswordAsync(request.EmployeeId, request.NewPassword);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
