using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApp.Shared.Model;

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
    public DbSet<Company> Companies { get; set; } = default!;

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

        // ===== COMPANY CONFIG =====
        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Code).HasMaxLength(50);
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

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.Employees)
                  .HasForeignKey(e => e.CompanyId)
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

            entity.Property(lt => lt.DaysPerYear).HasPrecision(7, 2);
            entity.Property(lt => lt.DaysPerCycle).HasPrecision(7, 2);
            entity.Property(lt => lt.PaymentPercentage).HasPrecision(5, 2);

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

            entity.Property(lb => lb.OpeningBalance)
                  .HasPrecision(9, 2);

            entity.Property(lb => lb.CurrentBalance)
                  .HasPrecision(9, 2);

            entity.HasOne(lb => lb.Employee)
                  .WithMany()
                  .HasForeignKey(lb => lb.EmployeeId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lb => lb.LeaveType)
                  .WithMany()
                  .HasForeignKey(lb => lb.LeaveTypeId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(lb => new { lb.EmployeeId, lb.LeaveTypeId })
                  .IsUnique();
        });

        // ===== SEED DATA =====
        SeedDepartments(modelBuilder);
        SeedIdentityUser(modelBuilder);
        SeedCompanies(modelBuilder);
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

    private void SeedCompanies(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>().HasData(
            new Company { Id = 1, Name = "AICS", Code = "AICS", IsActive = true },
            new Company { Id = 2, Name = "WFE", Code = "WFE", IsActive = true },
            new Company { Id = 3, Name = "TRICON", Code = "TRICON", IsActive = true },
            new Company { Id = 4, Name = "THEOUTPOST", Code = "THEOUTPOST", IsActive = true },
            new Company { Id = 5, Name = "AAS", Code = "AAS", IsActive = true }
        );
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
                CompanyId = 1,             // AICS
                Gender = Gender.Female,    // if you want a default
                IdentityUserId = "seed-admin-user-id",
                Role = "Admin",
                PasswordHash = dummyHash,
                PasswordSalt = dummySalt
            }
        );

        modelBuilder.Entity<LeaveType>().HasData(
            new LeaveType
            {
                Id = new Guid("28606668-B6E2-431D-9785-1FB3AB3DAFE6"),
                Name = "Maternity Leave",
                AccrualType = LeaveAccrualType.Fixed,
                DaysPerYear = 0m,
                DaysPerCycle = 0m,
                PaymentPercentage = 100m,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = new Guid("6F90F1BF-A17F-48B9-8DC5-5B63374D205F"),
                Name = "Study Leave",
                AccrualType = LeaveAccrualType.Fixed,
                DaysPerYear = 0m,
                DaysPerCycle = 0m,
                PaymentPercentage = 100m,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = new Guid("7F80A868-B953-46D7-8A95-6DD2319AE491"),
                Name = "Unpaid Leave",
                AccrualType = LeaveAccrualType.None,
                DaysPerYear = 0m,
                DaysPerCycle = 0m,
                PaymentPercentage = 0m,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = new Guid("F929540C-4B73-4E0C-B5D0-845C6A2FC4CF"),
                Name = "Family Responsibility Leave",
                AccrualType = LeaveAccrualType.Fixed,
                DaysPerYear = 0m,
                DaysPerCycle = 0m,
                PaymentPercentage = 100m,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = new Guid("34DEEBAB-CEA1-42D3-A537-B45BFB594AAA"),
                Name = "Sick Leave",
                AccrualType = LeaveAccrualType.Cycle,
                DaysPerYear = 0m,
                DaysPerCycle = 0m,
                PaymentPercentage = 100m,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = new Guid("FFF0BDDB-42BA-4CAB-8CC7-D02A6EE5B1C1"),
                Name = "Annual Leave",
                AccrualType = LeaveAccrualType.Annual,
                DaysPerYear = 15m,
                DaysPerCycle = 0m,
                PaymentPercentage = 100m,
                IsDeleted = false
            },
            new LeaveType
            {
                Id = new Guid("BADD5389-B6D5-4032-8A42-FBC9939C7AB4"),
                Name = "Paternity Leave",
                AccrualType = LeaveAccrualType.Fixed,
                DaysPerYear = 0m,
                DaysPerCycle = 0m,
                PaymentPercentage = 100m,
                IsDeleted = false
            }
        );
    }
}
