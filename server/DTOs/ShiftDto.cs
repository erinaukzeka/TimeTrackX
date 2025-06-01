using System;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.DTOs
{
    public class CreateShiftDto
    {
        public ShiftType Type { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Description { get; set; }
    }

    public class UpdateShiftDto
    {
        public ShiftType? Type { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Description { get; set; }
        public bool? IsActive { get; set; }
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
    }
} 