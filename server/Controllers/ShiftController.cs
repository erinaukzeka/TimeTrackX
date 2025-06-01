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
                .Select(s => new ShiftResponseDto
                {
                    Id = s.Id,
                    Type = s.Type,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return Ok(shifts);
        }

        // GET: api/shift/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftResponseDto>> GetShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);

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
                UpdatedAt = shift.UpdatedAt
            };

            return Ok(shiftDto);
        }

        // POST: api/shift
        [HttpPost]
        public async Task<ActionResult<ShiftResponseDto>> CreateShift(CreateShiftDto createShiftDto)
        {
            var shift = new Shift
            {
                Type = createShiftDto.Type,
                StartTime = createShiftDto.StartTime,
                EndTime = createShiftDto.EndTime,
                Description = createShiftDto.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            var shiftDto = new ShiftResponseDto
            {
                Id = shift.Id,
                Type = shift.Type,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Description = shift.Description,
                IsActive = shift.IsActive,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt
            };

            return CreatedAtAction(nameof(GetShift), new { id = shift.Id }, shiftDto);
        }

        // PUT: api/shift/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, UpdateShiftDto updateShiftDto)
        {
            var shift = await _context.Shifts.FindAsync(id);

            if (shift == null)
            {
                return NotFound();
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

        // DELETE: api/shift/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(e => e.Id == id);
        }
    }
} 