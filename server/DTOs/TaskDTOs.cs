using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.DTOs
{
    public class CreateTaskDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } = "Todo";

        [Required]
        [Range(1, 3)]
        public int Priority { get; set; } = 1;

        public DateTime? DueDate { get; set; }

        [Required]
        public int ProjectId { get; set; }

        public int? AssignedUserId { get; set; }
    }

    public class UpdateTaskDTO
    {
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Status { get; set; }

        [Range(1, 3)]
        public int? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public int? ProjectId { get; set; }

        public int? AssignedUserId { get; set; }
    }

    public class TaskResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? AssignedUserId { get; set; }
        public string AssignedUserName { get; set; }
    }

    public class UpdateTaskStatusDTO
    {
        [Required]
        public string Status { get; set; }
    }

    public class AssignTaskDTO
    {
        public int? UserId { get; set; }
    }
} 