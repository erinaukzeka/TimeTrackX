using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.Models
{
    public class TimeEntry
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int? TaskId { get; set; }
        public ProjectTask Task { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public bool IsRunning => EndTime == null;

        public TimeSpan Duration => EndTime.HasValue 
            ? EndTime.Value - StartTime 
            : DateTime.Now - StartTime;
    }
} 