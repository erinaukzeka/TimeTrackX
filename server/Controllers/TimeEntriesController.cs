using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;
using TimeTrackX.API.DTOs;
using TimeTrackX.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeEntriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ShiftValidationService _shiftValidation;

        public TimeEntriesController(ApplicationDbContext context, ShiftValidationService shiftValidation)
        {
            _context = context;
            _shiftValidation = shiftValidation;
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            // Try parsing as int first
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            // If it's not a number, try to find the user by username
            var user = _context.Users.FirstOrDefault(u => u.Username == userIdClaim);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid user identifier in token");
            }

            return user.Id;
        }

        // GET: api/TimeEntries
        [HttpGet]
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
        public async Task<ActionResult<TimeEntryResponseDTO>> CreateTimeEntry(CreateTimeEntryDTO dto)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                
                // Validate the time entry against shift schedule
                var validation = await _shiftValidation.ValidateTimeEntry(userId, dto.StartTime);
                if (!validation.IsValid)
                {
                    return BadRequest(validation.ErrorMessage);
                }

                var timeEntry = new TimeEntry
                {
                    UserId = userId,
                    ProjectId = dto.ProjectId,
                    TaskId = dto.TaskId,
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Description = dto.Description
                };

                _context.TimeEntries.Add(timeEntry);
                await _context.SaveChangesAsync();

                // Reload the entry with related data
                timeEntry = await _context.TimeEntries
                    .Include(t => t.User)
                    .Include(t => t.Project)
                    .Include(t => t.Task)
                    .FirstAsync(t => t.Id == timeEntry.Id);

                var response = new TimeEntryResponseDTO
                {
                    Id = timeEntry.Id,
                    UserId = timeEntry.UserId,
                    UserName = timeEntry.User?.Username,
                    ProjectId = timeEntry.ProjectId,
                    ProjectName = timeEntry.Project?.Name,
                    TaskId = timeEntry.TaskId,
                    TaskName = timeEntry.Task?.Name,
                    StartTime = timeEntry.StartTime,
                    EndTime = timeEntry.EndTime,
                    Description = timeEntry.Description,
                    Duration = timeEntry.EndTime.HasValue 
                        ? (timeEntry.EndTime.Value - timeEntry.StartTime).TotalHours 
                        : 0
                };

                return CreatedAtAction(nameof(GetTimeEntry), new { id = timeEntry.Id }, response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/TimeEntries/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTimeEntry(int id, UpdateTimeEntryDTO dto)
        {
            try
            {
                var timeEntry = await _context.TimeEntries.FindAsync(id);
                if (timeEntry == null)
                {
                    return NotFound();
                }

                // Verify the user owns this time entry or is an admin
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId) || userId == 0)
                {
                    return Unauthorized();
                }

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

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the time entry", details = ex.Message });
            }
        }

        // PUT: api/TimeEntries/5/stop
        [HttpPut("{id}/stop")]
        [Authorize]
        public async Task<IActionResult> StopTimeEntry(int id)
        {
            try
            {
                var timeEntry = await _context.TimeEntries.FindAsync(id);
                if (timeEntry == null)
                {
                    return NotFound();
                }

                // Verify the user owns this time entry or is an admin
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId) || userId == 0)
                {
                    return Unauthorized();
                }

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
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while stopping the time entry", details = ex.Message });
            }
        }

        // DELETE: api/TimeEntries/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTimeEntry(int id)
        {
            try
            {
                var timeEntry = await _context.TimeEntries.FindAsync(id);
                if (timeEntry == null)
                {
                    return NotFound();
                }

                // Verify the user owns this time entry or is an admin
                if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out int userId) || userId == 0)
                {
                    return Unauthorized();
                }

                if (timeEntry.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                _context.TimeEntries.Remove(timeEntry);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the time entry", details = ex.Message });
            }
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