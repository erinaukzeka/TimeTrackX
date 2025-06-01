using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.Services
{
    public class ShiftValidationService
    {
        private readonly ApplicationDbContext _context;

        public ShiftValidationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateTimeEntry(int userId, DateTime startTime)
        {
            // Convert DateTime to TimeSpan for comparison with shift times
            var timeOfDay = startTime.TimeOfDay;

            // Get the user's assigned shift for the current day
            var shift = await _context.Shifts
                .Include(s => s.AssignedEmployees)
                .Where(s => s.AssignedEmployees.Any(u => u.Id == userId))
                .Where(s => s.IsActive)
                .FirstOrDefaultAsync();

            if (shift == null)
            {
                return (false, "No active shift assigned for this employee.");
            }

            // Allow check-in 15 minutes before shift starts and 15 minutes after shift ends
            var gracePeriod = TimeSpan.FromMinutes(15);
            var earliestCheckIn = shift.StartTime - gracePeriod;
            var latestCheckIn = shift.EndTime + gracePeriod;

            if (timeOfDay < earliestCheckIn || timeOfDay > latestCheckIn)
            {
                return (false, $"Time entry is outside of your scheduled shift ({shift.StartTime:hh\\:mm} - {shift.EndTime:hh\\:mm}). Please contact your supervisor if you need to work outside your scheduled hours.");
            }

            return (true, null);
        }

        public async Task<(bool IsValid, string ErrorMessage)> ValidateTimeEntryUpdate(int userId, DateTime? startTime, DateTime? endTime)
        {
            if (startTime.HasValue)
            {
                var startValidation = await ValidateTimeEntry(userId, startTime.Value);
                if (!startValidation.IsValid)
                {
                    return startValidation;
                }
            }

            if (endTime.HasValue)
            {
                var endValidation = await ValidateTimeEntry(userId, endTime.Value);
                if (!endValidation.IsValid)
                {
                    return endValidation;
                }
            }

            return (true, null);
        }
    }
} 