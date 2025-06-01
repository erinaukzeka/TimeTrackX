using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;
using TimeTrackX.API.DTOs;
using TimeTrackX.API.Services;
using Microsoft.AspNetCore.Authorization;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ShiftValidationService _shiftValidation;

        public TimeEntriesController(ApplicationDbContext context, ShiftValidationService shiftValidation)
        {
            _context = context;
            _shiftValidation = shiftValidation;
        }

        // GET: api/TimeEntries
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimeEntryResponseDTO>>> GetTimeEntries()
        {
            var entries = await _context.TimeEntries
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Task)
                .ToListAsync();

            return entries.Select(MapToResponseDTO).ToList();
        }

        // GET: api/TimeEntries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeEntryResponseDTO>> GetTimeEntry(int id)
        {
            var timeEntry = await _context.TimeEntries
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Task)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (timeEntry == null)
            {
                return NotFound();
            }

            return MapToResponseDTO(timeEntry);
        }

        // GET: api/TimeEntries/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TimeEntryResponseDTO>>> GetUserTimeEntries(int userId)
        {
            var entries = await _context.TimeEntries
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Task)
                .Where(t => t.UserId == userId)
                .ToListAsync();

            return entries.Select(MapToResponseDTO).ToList();
        }

        // POST: api/TimeEntries
        [HttpPost]
        public async Task<ActionResult<TimeEntryResponseDTO>> CreateTimeEntry(CreateTimeEntryDTO dto)
        {
            // Get the current user's ID from the token
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (userId == 0)
            {
                return Unauthorized();
            }

            // Validate against scheduled shift
            var validation = await _shiftValidation.ValidateTimeEntry(userId, dto.StartTime);
            if (!validation.IsValid)
            {
                return BadRequest(validation.ErrorMessage);
            }

            var timeEntry = new TimeEntry
            {
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                TaskId = dto.TaskId,
                UserId = userId
            };

            _context.TimeEntries.Add(timeEntry);
            await _context.SaveChangesAsync();

            // Reload the entry with related entities
            timeEntry = await _context.TimeEntries
                .Include(t => t.User)
                .Include(t => t.Project)
                .Include(t => t.Task)
                .FirstAsync(t => t.Id == timeEntry.Id);

            return CreatedAtAction(
                nameof(GetTimeEntry),
                new { id = timeEntry.Id },
                MapToResponseDTO(timeEntry));
        }

        // PUT: api/TimeEntries/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeEntry(int id, UpdateTimeEntryDTO dto)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(id);
            if (timeEntry == null)
            {
                return NotFound();
            }

            // Verify the user owns this time entry or is an admin
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (timeEntry.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Validate against scheduled shift
            var validation = await _shiftValidation.ValidateTimeEntryUpdate(
                timeEntry.UserId,
                dto.StartTime,
                dto.EndTime);

            if (!validation.IsValid)
            {
                return BadRequest(validation.ErrorMessage);
            }

            // Only update provided fields
            if (dto.StartTime.HasValue) timeEntry.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) timeEntry.EndTime = dto.EndTime;
            if (dto.Description != null) timeEntry.Description = dto.Description;
            if (dto.ProjectId.HasValue) timeEntry.ProjectId = dto.ProjectId.Value;
            if (dto.TaskId.HasValue) timeEntry.TaskId = dto.TaskId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimeEntryExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // PUT: api/TimeEntries/5/stop
        [HttpPut("{id}/stop")]
        public async Task<IActionResult> StopTimeEntry(int id)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(id);
            if (timeEntry == null)
            {
                return NotFound();
            }

            // Verify the user owns this time entry or is an admin
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (timeEntry.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var stopTime = DateTime.UtcNow;
            
            // Validate against scheduled shift
            var validation = await _shiftValidation.ValidateTimeEntry(timeEntry.UserId, stopTime);
            if (!validation.IsValid)
            {
                return BadRequest(validation.ErrorMessage);
            }

            timeEntry.EndTime = stopTime;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/TimeEntries/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeEntry(int id)
        {
            var timeEntry = await _context.TimeEntries.FindAsync(id);
            if (timeEntry == null)
            {
                return NotFound();
            }

            // Verify the user owns this time entry or is an admin
            var userId = int.Parse(User.FindFirst("sub")?.Value ?? "0");
            if (timeEntry.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.TimeEntries.Remove(timeEntry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TimeEntryExists(int id)
        {
            return _context.TimeEntries.Any(e => e.Id == id);
        }

        private static TimeEntryResponseDTO MapToResponseDTO(TimeEntry entry)
        {
            var duration = 0.0;
            if (entry.EndTime.HasValue)
            {
                duration = (entry.EndTime.Value - entry.StartTime).TotalHours;
            }

            return new TimeEntryResponseDTO
            {
                Id = entry.Id,
                StartTime = entry.StartTime,
                EndTime = entry.EndTime,
                Description = entry.Description,
                UserId = entry.UserId,
                UserName = entry.User?.Username,
                ProjectId = entry.ProjectId,
                ProjectName = entry.Project?.Name,
                TaskId = entry.TaskId,
                TaskName = entry.Task?.Name,
                Duration = duration
            };
        }
    }
} 