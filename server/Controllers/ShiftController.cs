using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.DTOs;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Ensure only admins can access shift management
    public class ShiftController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/shift
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftResponseDto>>> GetShifts()
        {
            var shifts = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .Select(s => new ShiftResponseDto
                {
                    Id = s.Id,
                    Type = s.Type,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt,
                    AssignedEmployees = s.AssignedEmployees.Select(u => new UserBasicInfoDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email
                    }).ToList()
                })
                .ToListAsync();

            return Ok(shifts);
        }

        // GET: api/shift/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftResponseDto>> GetShift(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound();
            }

            var shiftDto = new ShiftResponseDto
            {
                Id = shift.Id,
                Type = shift.Type,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Description = shift.Description,
                IsActive = shift.IsActive,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt,
                AssignedEmployees = shift.AssignedEmployees.Select(u => new UserBasicInfoDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email
                }).ToList()
            };

            return Ok(shiftDto);
        }

        // POST: api/shift
        [HttpPost]
        public async Task<ActionResult<ShiftResponseDto>> CreateShift(CreateShiftDto createShiftDto)
        {
            // Validate for overlapping shifts
            if (createShiftDto.AssignedEmployeeIds?.Any() == true)
            {
                var hasOverlap = await CheckForOverlappingShifts(
                    createShiftDto.AssignedEmployeeIds,
                    createShiftDto.StartTime,
                    createShiftDto.EndTime,
                    null);

                if (hasOverlap)
                {
                    return BadRequest("One or more employees have overlapping shifts during this time period.");
                }
            }

            var shift = new Shift
            {
                Type = createShiftDto.Type,
                StartTime = createShiftDto.StartTime,
                EndTime = createShiftDto.EndTime,
                Description = createShiftDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            if (createShiftDto.AssignedEmployeeIds?.Any() == true)
            {
                var employees = await _context.Users
                    .Where(u => createShiftDto.AssignedEmployeeIds.Contains(u.Id))
                    .ToListAsync();
                
                foreach (var employee in employees)
                {
                    shift.AssignedEmployees.Add(employee);
                }
            }

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            // Reload the shift with assigned employees
            await _context.Entry(shift)
                .Collection(s => s.AssignedEmployees)
                .LoadAsync();

            var shiftDto = new ShiftResponseDto
            {
                Id = shift.Id,
                Type = shift.Type,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Description = shift.Description,
                IsActive = shift.IsActive,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt,
                AssignedEmployees = shift.AssignedEmployees.Select(u => new UserBasicInfoDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email
                }).ToList()
            };

            return CreatedAtAction(nameof(GetShift), new { id = shift.Id }, shiftDto);
        }

        // PUT: api/shift/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, UpdateShiftDto updateShiftDto)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound();
            }

            // Validate for overlapping shifts if time or assignments are being updated
            if (updateShiftDto.AssignedEmployeeIds != null || 
                updateShiftDto.StartTime.HasValue || 
                updateShiftDto.EndTime.HasValue)
            {
                var startTime = updateShiftDto.StartTime ?? shift.StartTime;
                var endTime = updateShiftDto.EndTime ?? shift.EndTime;
                var employeeIds = updateShiftDto.AssignedEmployeeIds ?? 
                    shift.AssignedEmployees.Select(e => e.Id).ToList();

                var hasOverlap = await CheckForOverlappingShifts(
                    employeeIds,
                    startTime,
                    endTime,
                    id);

                if (hasOverlap)
                {
                    return BadRequest("One or more employees have overlapping shifts during this time period.");
                }
            }

            if (updateShiftDto.Type.HasValue)
                shift.Type = updateShiftDto.Type.Value;
            if (updateShiftDto.StartTime.HasValue)
                shift.StartTime = updateShiftDto.StartTime.Value;
            if (updateShiftDto.EndTime.HasValue)
                shift.EndTime = updateShiftDto.EndTime.Value;
            if (updateShiftDto.Description != null)
                shift.Description = updateShiftDto.Description;
            if (updateShiftDto.IsActive.HasValue)
                shift.IsActive = updateShiftDto.IsActive.Value;

            if (updateShiftDto.AssignedEmployeeIds != null)
            {
                shift.AssignedEmployees.Clear();
                var employees = await _context.Users
                    .Where(u => updateShiftDto.AssignedEmployeeIds.Contains(u.Id))
                    .ToListAsync();
                
                foreach (var employee in employees)
                {
                    shift.AssignedEmployees.Add(employee);
                }
            }

            shift.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShiftExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // POST: api/shift/{id}/assign
        [HttpPost("{id}/assign")]
        public async Task<IActionResult> AssignEmployees(int id, ShiftAssignmentDto assignmentDto)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound();
            }

            // Validate for overlapping shifts
            var hasOverlap = await CheckForOverlappingShifts(
                assignmentDto.EmployeeIds,
                shift.StartTime,
                shift.EndTime,
                id);

            if (hasOverlap)
            {
                return BadRequest("One or more employees have overlapping shifts during this time period.");
            }

            var employees = await _context.Users
                .Where(u => assignmentDto.EmployeeIds.Contains(u.Id))
                .ToListAsync();

            foreach (var employee in employees)
            {
                if (!shift.AssignedEmployees.Any(e => e.Id == employee.Id))
                {
                    shift.AssignedEmployees.Add(employee);
                }
            }

            shift.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/shift/{id}/unassign
        [HttpPost("{id}/unassign")]
        public async Task<IActionResult> UnassignEmployees(int id, ShiftAssignmentDto unassignmentDto)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound();
            }

            var employeesToRemove = shift.AssignedEmployees
                .Where(e => unassignmentDto.EmployeeIds.Contains(e.Id))
                .ToList();

            foreach (var employee in employeesToRemove)
            {
                shift.AssignedEmployees.Remove(employee);
            }

            shift.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/shift/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
            {
                return NotFound();
            }

            shift.AssignedEmployees.Clear();
            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.Id == id);
        }

        private async Task<bool> CheckForOverlappingShifts(
            List<int> employeeIds,
            TimeSpan startTime,
            TimeSpan endTime,
            int? excludeShiftId)
        {
            var query = _context.Shifts
                .Include(s => s.AssignedEmployees)
                .Where(s => s.IsActive);

            if (excludeShiftId.HasValue)
            {
                query = query.Where(s => s.Id != excludeShiftId.Value);
            }

            var existingShifts = await query.ToListAsync();

            foreach (var shift in existingShifts)
            {
                if (shift.StartTime < endTime && startTime < shift.EndTime)
                {
                    // Check if any of the employees are assigned to this overlapping shift
                    if (shift.AssignedEmployees.Any(e => employeeIds.Contains(e.Id)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
} 