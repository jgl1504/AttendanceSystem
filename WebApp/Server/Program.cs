using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApp.Server.Data;
using WebApp.Server.Services;
using WebApp.Server.Services.Attendance;
using WebApp.Server.Services.Departments;
using WebApp.Server.Services.Employees;
using WebApp.Server.Services.Leave;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (even if you don't use UI yet)
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<DataContext>();

// JWT settings from configuration
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
var jwtIssuer = jwtSection["Issuer"];
var jwtAudience = jwtSection["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is not configured in appsettings.json");

// JWT authentication
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
        ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience
    };
});

// MVC / API / Blazor
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// App services
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<DepartmentService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LeaveService>();
builder.Services.AddScoped<SiteService>();
builder.Services.AddScoped<LeaveTypeService>();
builder.Services.AddScoped<LeaveBalanceService>();
builder.Services.AddScoped<LeaveService>();
builder.Services.AddScoped<LeaveRequestService>();



builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// TEMP endpoint to generate hash for password "Newl0gin"
//app.MapGet("/debug/hash", () =>
//{
//    var user = new IdentityUser { UserName = "Projects@aics.co.za" };
//    var hasher = new PasswordHasher<IdentityUser>();

//    var hash = hasher.HashPassword(user, "Newl0gin");
//    return Results.Text(hash);
//});

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
