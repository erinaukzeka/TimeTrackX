using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.Models
{
    public class ProjectTask
    {
        public ProjectTask()
        {
            TimeEntries = new List<TimeEntry>();
            CreatedAt = DateTime.UtcNow;
            Status = "Todo";
            Priority = 1;
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        [Range(1, 3)]
        public int Priority { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? DueDate { get; set; }

        // Foreign keys
        public int ProjectId { get; set; }
        public int? AssignedUserId { get; set; }

        // Navigation properties
        public Project Project { get; set; }
        public User AssignedUser { get; set; }
        public ICollection<TimeEntry> TimeEntries { get; set; }
    }
} 