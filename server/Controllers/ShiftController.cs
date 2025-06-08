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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftResponseDto>>> GetShifts()
        {
            var shifts = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .ToListAsync();

            // Order the shifts in memory after fetching from database
            var orderedShifts = shifts
                .OrderBy(s => s.StartTime.TotalMinutes)
                .Select(shift => new ShiftResponseDto
                {
                    Id = shift.Id,
                    Type = shift.Type,
                    StartTime = shift.StartTime,
                    EndTime = shift.EndTime,
                    Description = shift.Description,
                    IsActive = shift.IsActive,
                    CreatedAt = shift.CreatedAt,
                    UpdatedAt = shift.UpdatedAt,
                    AssignedEmployees = shift.AssignedEmployees.Select(u => new UserDto
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                        Role = u.Role
                    }).ToList()
                })
                .ToList();

            return orderedShifts;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftResponseDto>> GetShift(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return NotFound();

            return new ShiftResponseDto
            {
                Id = shift.Id,
                Type = shift.Type,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Description = shift.Description,
                IsActive = shift.IsActive,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt,
                AssignedEmployees = shift.AssignedEmployees.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                }).ToList()
            };
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShiftResponseDto>> CreateShift(CreateShiftDto createShiftDto)
        {
            var shift = new Shift
            {
                Type = createShiftDto.Type,
                StartTime = createShiftDto.StartTime,
                EndTime = createShiftDto.EndTime,
                Description = createShiftDto.Description,
                IsActive = true
            };

            if (createShiftDto.AssignedEmployeeIds?.Any() == true)
            {
                var employees = await _context.Users
                    .Where(u => createShiftDto.AssignedEmployeeIds.Contains(u.Id))
                    .ToListAsync();
                shift.AssignedEmployees = employees;
            }

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return await GetShift(shift.Id);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ShiftResponseDto>> UpdateShift(int id, UpdateShiftDto updateShiftDto)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return NotFound();

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
                var employees = await _context.Users
                    .Where(u => updateShiftDto.AssignedEmployeeIds.Contains(u.Id))
                    .ToListAsync();
                shift.AssignedEmployees.Clear();
                shift.AssignedEmployees = employees;
            }

            shift.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetShift(shift.Id);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
                return NotFound();

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/assign/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AssignEmployee(int id, int userId)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return NotFound("Shift not found");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            if (shift.AssignedEmployees.Any(e => e.Id == userId))
                return BadRequest("User is already assigned to this shift");

            shift.AssignedEmployees.Add(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}/assign/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> UnassignEmployee(int id, int userId)
        {
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (shift == null)
                return NotFound("Shift not found");

            var user = shift.AssignedEmployees.FirstOrDefault(e => e.Id == userId);
            if (user == null)
                return NotFound("User is not assigned to this shift");

            shift.AssignedEmployees.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
} 