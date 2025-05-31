using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.DTOs
{
    public class CreateTimeEntryDTO
    {
        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? TaskId { get; set; }
    }

    public class UpdateTimeEntryDTO
    {
        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Description { get; set; }

        public int? ProjectId { get; set; }

        public int? TaskId { get; set; }
    }

    public class TimeEntryResponseDTO
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? TaskId { get; set; }
        public string TaskName { get; set; }
        public double Duration { get; set; } // Duration in hours
    }
} 