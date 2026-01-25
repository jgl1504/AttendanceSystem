using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Role { get; set; } = "Employee";
    }
}
