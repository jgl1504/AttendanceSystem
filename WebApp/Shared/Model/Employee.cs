using System;

namespace WebApp.Shared.Model
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public int DepartmentId { get; set; }          // NOT nullable
        public Department? Department { get; set; }    // nav can stay nullable

        // NEW: Gender
        public Gender Gender { get; set; } = Gender.Other;

        // NEW: Company / Tenant
        public int CompanyId { get; set; }
        public Company? Company { get; set; }


        // Auth fields (for JWT)
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public string Role { get; set; } = "Employee";

        public string? IdentityUserId { get; set; }
        public string? UserId { get; set; }
    }
}
