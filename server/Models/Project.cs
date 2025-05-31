using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.Models
{
    public class Project
    {
        public Project()
        {
            TimeEntries = new List<TimeEntry>();
            Tasks = new List<ProjectTask>();
            AssignedUsers = new List<User>();
            CreatedAt = DateTime.UtcNow;
            Status = "Active";
        }

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public ICollection<TimeEntry> TimeEntries { get; set; }
        public ICollection<ProjectTask> Tasks { get; set; }
        public ICollection<User> AssignedUsers { get; set; }
    }
} 