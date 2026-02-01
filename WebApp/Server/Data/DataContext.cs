using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApp.Shared.Model;
using System.Security.Cryptography;
using System.Text;

namespace WebApp.Server.Data;

public class DataContext : IdentityDbContext<IdentityUser>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; } = default!;
    public DbSet<Employee> Employees { get; set; } = default!;
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; } = default!;
    public DbSet<EmployeeLeave> EmployeeLeaves { get; set; } = default!;
    public DbSet<LeaveRecord> LeaveRecords { get; set; } = default!;
    public DbSet<Site> Sites { get; set; } = default!;
    public DbSet<LeaveType> LeaveTypes { get; set; } = default!;
    public DbSet<LeaveBalance> LeaveBalances { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables

        // ===== DEPARTMENT CONFIG =====
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
            entity.Property(d => d.RequiredHoursPerWeek).HasPrecision(5, 2);
            entity.Property(d => d.SaturdayHours).HasPrecision(4, 2);
            entity.Property(d => d.SundayHours).HasPrecision(4, 2);
        });

        // ===== EMPLOYEE CONFIG =====
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.IdentityUserId).HasMaxLength(450);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.PasswordSalt).IsRequired();

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== ATTENDANCE RECORD CONFIG =====
        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            entity.HasKey(a => a.Id);

            entity.HasOne(a => a.Employee)
                  .WithMany()
                  .HasForeignKey(a => a.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.ClockedByEmployee)
                  .WithMany()
                  .HasForeignKey(a => a.ClockedByEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.OvertimeApprovedByEmployee)
                  .WithMany()
                  .HasForeignKey(a => a.OvertimeApprovedByEmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.Site)
                  .WithMany()
                  .HasForeignKey(a => a.SiteId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== EMPLOYEE LEAVE CONFIG =====
        modelBuilder.Entity<EmployeeLeave>(entity =>
        {
            entity.HasKey(el => el.Id);
            entity.Property(el => el.DaysBalance).HasPrecision(7, 2);
            entity.Property(el => el.AccrualRatePerMonth).HasPrecision(5, 2);

            entity.HasOne(el => el.Employee)
                  .WithMany()
                  .HasForeignKey(el => el.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== LEAVE RECORD CONFIG =====
        modelBuilder.Entity<LeaveRecord>(entity =>
        {
            entity.HasKey(lr => lr.Id);
            entity.Property(lr => lr.DaysTaken).HasPrecision(7, 2);

            entity.HasOne(lr => lr.Employee)
                  .WithMany()
                  .HasForeignKey(lr => lr.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lr => lr.LeaveType)
                  .WithMany()
                  .HasForeignKey(lr => lr.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== LEAVE TYPE CONFIG =====
        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(lt => lt.Id);

            // Precision for decimal fields
            entity.Property(lt => lt.DaysPerYear).HasPrecision(7, 2);
            entity.Property(lt => lt.DaysPerCycle).HasPrecision(7, 2);
            entity.Property(lt => lt.PaymentPercentage).HasPrecision(5, 2);

            // Self-referencing for pool relationships
            entity.HasOne(lt => lt.PrimaryPoolLeaveType)
                  .WithMany()
                  .HasForeignKey(lt => lt.PrimaryPoolLeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(lt => lt.FallbackPoolLeaveType)
                  .WithMany()
                  .HasForeignKey(lt => lt.FallbackPoolLeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== LEAVE BALANCE CONFIG =====
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(lb => lb.Id);

            // Precision for decimal fields
            entity.Property(lb => lb.OpeningBalance)
                  .HasPrecision(9, 2);

            entity.Property(lb => lb.CurrentBalance)
                  .HasPrecision(9, 2); // same precision for both

            entity.HasOne(lb => lb.Employee)
                  .WithMany()
                  .HasForeignKey(lb => lb.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lb => lb.LeaveType)
                  .WithMany()
                  .HasForeignKey(lb => lb.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: one balance per employee per leave type
            entity.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId })
                  .IsUnique();
        });


        // ===== SEED DATA =====
        SeedDepartments(modelBuilder);
        SeedIdentityUser(modelBuilder);
        SeedEmployees(modelBuilder);
    }

    private void SeedDepartments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>().HasData(
            new Department
            {
                Id = 1,
                Name = "Workshop",
                RequiredHoursPerWeek = 45m,
                DailyStartTime = new TimeSpan(7, 30, 0),
                DailyEndTime = new TimeSpan(17, 0, 0),
                BreakPerDay = new TimeSpan(1, 0, 0),
                WorksSaturday = true,
                RotatingWeekends = true,
                SaturdaysPerMonthRequired = 3,
                SaturdayHours = 5m,
                WorksSunday = false,
                SundayHours = 0m,
                GraceMinutesBefore = 10,
                GraceMinutesAfter = 10,
                AllowOvertime = true
            },
            new Department
            {
                Id = 2,
                Name = "Onsite",
                RequiredHoursPerWeek = 45m,
                DailyStartTime = new TimeSpan(8, 0, 0),
                DailyEndTime = new TimeSpan(17, 30, 0),
                BreakPerDay = new TimeSpan(1, 0, 0),
                WorksSaturday = true,
                RotatingWeekends = true,
                SaturdaysPerMonthRequired = 3,
                SaturdayHours = 5m,
                WorksSunday = false,
                SundayHours = 0m,
                GraceMinutesBefore = 15,
                GraceMinutesAfter = 15,
                AllowOvertime = true
            },
            new Department
            {
                Id = 3,
                Name = "Back Office",
                RequiredHoursPerWeek = 40m,
                DailyStartTime = new TimeSpan(8, 0, 0),
                DailyEndTime = new TimeSpan(16, 30, 0),
                BreakPerDay = new TimeSpan(1, 0, 0),
                WorksSaturday = false,
                RotatingWeekends = false,
                SaturdaysPerMonthRequired = 0,
                SaturdayHours = 0m,
                WorksSunday = false,
                SundayHours = 0m,
                GraceMinutesBefore = 5,
                GraceMinutesAfter = 5,
                AllowOvertime = false
            }
        );
    }

    private void SeedIdentityUser(ModelBuilder modelBuilder)
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var adminUserId = "seed-admin-user-id";

        var identityAdmin = new IdentityUser
        {
            Id = adminUserId,
            UserName = "Projects@aics.co.za",
            NormalizedUserName = "PROJECTS@AICS.CO.ZA",
            Email = "Projects@aics.co.za",
            NormalizedEmail = "PROJECTS@AICS.CO.ZA",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D")
        };

        identityAdmin.PasswordHash = hasher.HashPassword(identityAdmin, "Newl0gin");
        modelBuilder.Entity<IdentityUser>().HasData(identityAdmin);
    }

    private void SeedEmployees(ModelBuilder modelBuilder)
    {
        var dummyHash = Encoding.UTF8.GetBytes("seed-hash");
        var dummySalt = Encoding.UTF8.GetBytes("seed-salt");

        modelBuilder.Entity<Employee>().HasData(
            new Employee
            {
                Id = 1,
                Name = "Zuleicke Visser",
                Email = "Projects@aics.co.za",
                Phone = string.Empty,
                HireDate = new DateTime(2025, 1, 1),
                IsActive = true,
                DepartmentId = 3,
                IdentityUserId = "seed-admin-user-id",
                Role = "Admin",
                PasswordHash = dummyHash,
                PasswordSalt = dummySalt
            }
        );
    }
}
