using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.DTOs
{
    public class CreateProjectDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } = "Active";

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class UpdateProjectDTO
    {
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }

    public class ProjectResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<int> AssignedUserIds { get; set; }
    }

    public class AssignUsersDTO
    {
        [Required]
        public ICollection<int> UserIds { get; set; }
    }
} 