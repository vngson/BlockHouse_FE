using System;

namespace BlockHouse.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int Status { get; set; } = 1; // Default to ACTIVE
        public double? Income { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public static class EmployeeStatus
        {
            public const int ACTIVE = 1;
            public const int INACTIVE = 0;
        }

        public override string ToString()
        {
            return $"<Employee {Name} (ID: {Id})>";
        }
    }
}