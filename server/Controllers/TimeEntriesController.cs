using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;
using TimeTrackX.API.DTOs;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimeEntriesController(ApplicationDbContext context)
        {
            _context = context;
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
            var timeEntry = new TimeEntry
            {
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                TaskId = dto.TaskId,
                UserId = 1 // TODO: Get from authenticated user
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

            timeEntry.EndTime = DateTime.UtcNow;
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