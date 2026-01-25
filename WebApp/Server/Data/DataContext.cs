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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Identity tables

        // Department config
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Name).IsRequired().HasMaxLength(200);

            // Work rules
            entity.Property(d => d.RequiredHoursPerWeek)
                  .HasPrecision(5, 2); // e.g. 45.00

            entity.Property(d => d.BreakPerDay)
                  .IsRequired();

            entity.Property(d => d.DailyStartTime)
                  .IsRequired();

            entity.Property(d => d.DailyEndTime)
                  .IsRequired();

            entity.Property(d => d.GraceMinutesBefore)
                  .IsRequired();

            entity.Property(d => d.GraceMinutesAfter)
                  .IsRequired();

            entity.Property(d => d.WorksSaturday)
                  .IsRequired();

            entity.Property(d => d.RotatingWeekends)
                  .IsRequired();

            entity.Property(d => d.SaturdaysPerMonthRequired)
                  .IsRequired();

            entity.Property(d => d.AllowOvertime)
                  .IsRequired();

            // Weekend hours mapping
            entity.Property(d => d.SaturdayHours)
                  .HasPrecision(4, 2);

            entity.Property(d => d.WorksSunday)
                  .IsRequired();

            entity.Property(d => d.SundayHours)
                  .HasPrecision(4, 2);
        });

        modelBuilder.Entity<EmployeeLeave>(b =>
        {
            b.Property(x => x.AccrualRatePerMonth).HasPrecision(5, 2); // e.g. 999.99
            b.Property(x => x.DaysBalance).HasPrecision(7, 2);         // e.g. 99999.99
        });

        modelBuilder.Entity<LeaveRecord>(b =>
        {
            b.Property(x => x.DaysTaken).HasPrecision(7, 2);
        });

        // Employee config (includes auth fields)
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.IdentityUserId).HasMaxLength(450); // link to AspNetUsers.Id

            entity.Property(e => e.Role)
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(e => e.PasswordHash)
                  .IsRequired();

            entity.Property(e => e.PasswordSalt)
                  .IsRequired();

            entity.HasOne(e => e.Department)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.DepartmentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Attendance config
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
        });

        // ----- SEED DATA -----

        // Departments
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

        // ---- Identity admin user seed (Projects@aics.co.za) ----
        var hasher = new PasswordHasher<IdentityUser>();
        var adminUserId = "seed-admin-user-id"; // must be stable string, not GUID.NewGuid()

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

        identityAdmin.PasswordHash = hasher.HashPassword(identityAdmin, "Newl0gin"); // change after first login

        modelBuilder.Entity<IdentityUser>().HasData(identityAdmin);

        // ---- Employee linked to that Identity user: Zuleicke Visser ----
        // For your custom PasswordHash/Salt fields, seed some dummy bytes; you will move away from them later.
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
                DepartmentId = 3,        // Back Office (or change if you prefer)
                IdentityUserId = adminUserId,

                Role = "Admin",
                PasswordHash = dummyHash,
                PasswordSalt = dummySalt
            }
        );
    }
}
