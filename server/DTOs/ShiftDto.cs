using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.DTOs
{
    public class CreateShiftDto
    {
        [Required]
        public ShiftType Type { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public string Description { get; set; }

        public List<int> AssignedEmployeeIds { get; set; } = new List<int>();
    }

    public class UpdateShiftDto
    {
        public ShiftType? Type { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
        
        [Required]
        public List<int> AssignedEmployeeIds { get; set; } = new List<int>();
    }

    public class ShiftResponseDto
    {
        public int Id { get; set; }
        public ShiftType Type { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<UserDto> AssignedEmployees { get; set; }
    }

    public class UserBasicInfoDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class ShiftAssignmentDto
    {
        public List<int> EmployeeIds { get; set; }
    }
} 