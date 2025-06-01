using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TimeTrackX.API.Models
{
    public enum ShiftType
    {
        Morning,
        Evening,
        Night
    }

    public class Shift : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public ShiftType Type { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property for assigned employees
        public ICollection<User> AssignedEmployees { get; set; } = new List<User>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "End time must be after start time",
                    new[] { nameof(EndTime) }
                );
            }
        }
    }
} 