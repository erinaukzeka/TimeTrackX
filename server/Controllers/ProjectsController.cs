using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;
using TimeTrackX.API.DTOs;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectResponseDTO>>> GetProjects()
        {
            var projects = await _context.Projects
                .Include(p => p.AssignedUsers)
                .Include(p => p.Tasks)
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                AssignedUserIds = p.AssignedUsers?.Select(u => u.Id).ToList() ?? new List<int>()
            }).ToList();
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectResponseDTO>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.AssignedUsers)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return new ProjectResponseDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt,
                AssignedUserIds = project.AssignedUsers?.Select(u => u.Id).ToList() ?? new List<int>()
            };
        }

        // GET: api/Projects/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ProjectResponseDTO>>> GetUserProjects(int userId)
        {
            var projects = await _context.Projects
                .Include(p => p.AssignedUsers)
                .Where(p => p.AssignedUsers.Any(u => u.Id == userId))
                .ToListAsync();

            return projects.Select(p => new ProjectResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Status = p.Status,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                CreatedAt = p.CreatedAt,
                AssignedUserIds = p.AssignedUsers?.Select(u => u.Id).ToList() ?? new List<int>()
            }).ToList();
        }

        // POST: api/Projects
        [HttpPost]
        public async Task<ActionResult<ProjectResponseDTO>> CreateProject(CreateProjectDTO dto)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                Status = dto.Status,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                AssignedUsers = new List<User>(),
                Tasks = new List<ProjectTask>(),
                TimeEntries = new List<TimeEntry>()
            };
            
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProject), 
                new { id = project.Id }, 
                new ProjectResponseDTO
                {
                    Id = project.Id,
                    Name = project.Name,
                    Description = project.Description,
                    Status = project.Status,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    CreatedAt = project.CreatedAt,
                    AssignedUserIds = new List<int>()
                });
        }

        // PUT: api/Projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, UpdateProjectDTO dto)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            // Only update provided fields
            if (dto.Name != null) project.Name = dto.Name;
            if (dto.Description != null) project.Description = dto.Description;
            if (dto.Status != null) project.Status = dto.Status;
            if (dto.StartDate.HasValue) project.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) project.EndDate = dto.EndDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/Projects/5/users
        [HttpPost("{id}/users")]
        public async Task<IActionResult> AssignUsers(int id, AssignUsersDTO dto)
        {
            var project = await _context.Projects
                .Include(p => p.AssignedUsers)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            if (project.AssignedUsers == null)
            {
                project.AssignedUsers = new List<User>();
            }

            var users = await _context.Users
                .Where(u => dto.UserIds.Contains(u.Id))
                .ToListAsync();

            foreach (var user in users)
            {
                if (!project.AssignedUsers.Any(u => u.Id == user.Id))
                {
                    project.AssignedUsers.Add(user);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
} 