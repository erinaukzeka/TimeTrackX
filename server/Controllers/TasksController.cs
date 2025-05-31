using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;
using TimeTrackX.API.DTOs;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TasksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskResponseDTO>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .ToListAsync();

            return tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name,
                AssignedUserId = t.AssignedUserId,
                AssignedUserName = t.AssignedUser?.Username
            }).ToList();
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDTO>> GetTask(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return new TaskResponseDTO
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.Username
            };
        }

        // GET: api/Tasks/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDTO>>> GetProjectTasks(int projectId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            return tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name,
                AssignedUserId = t.AssignedUserId,
                AssignedUserName = t.AssignedUser?.Username
            }).ToList();
        }

        // GET: api/Tasks/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskResponseDTO>>> GetUserTasks(int userId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId)
                .ToListAsync();

            return tasks.Select(t => new TaskResponseDTO
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                CreatedAt = t.CreatedAt,
                ProjectId = t.ProjectId,
                ProjectName = t.Project?.Name,
                AssignedUserId = t.AssignedUserId,
                AssignedUserName = t.AssignedUser?.Username
            }).ToList();
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<TaskResponseDTO>> CreateTask(CreateTaskDTO dto)
        {
            var task = new ProjectTask
            {
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
                Priority = dto.Priority,
                DueDate = dto.DueDate,
                ProjectId = dto.ProjectId,
                AssignedUserId = dto.AssignedUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Reload the task with related entities
            task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .FirstAsync(t => t.Id == task.Id);

            var response = new TaskResponseDTO
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CreatedAt = task.CreatedAt,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.Username
            };

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, response);
        }

        // PUT: api/Tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, UpdateTaskDTO dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            // Only update provided fields
            if (dto.Name != null) task.Name = dto.Name;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.Status != null) task.Status = dto.Status;
            if (dto.Priority.HasValue) task.Priority = dto.Priority.Value;
            if (dto.DueDate.HasValue) task.DueDate = dto.DueDate;
            if (dto.ProjectId.HasValue) task.ProjectId = dto.ProjectId.Value;
            if (dto.AssignedUserId.HasValue) task.AssignedUserId = dto.AssignedUserId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // PUT: api/Tasks/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateTaskStatusDTO dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Status = dto.Status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Tasks/5/assign
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(int id, AssignTaskDTO dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.AssignedUserId = dto.UserId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
} 