using System;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Shared.Model
{
    public class EmployeeDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]  // validates email format
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime HireDate { get; set; }

        public bool IsActive { get; set; }

        public int DepartmentId { get; set; }
        public DepartmentDto? Department { get; set; }

        // NEW: Gender (same enum as entity)
        public Gender Gender { get; set; } = Gender.Other;

        // NEW: Company
        public int CompanyId { get; set; }
        public CompanyDto? Company { get; set; }

        public string Role { get; set; } = "Employee";
    }
}
