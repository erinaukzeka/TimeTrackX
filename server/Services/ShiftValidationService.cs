using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.Services
{
    public class ShiftValidationService
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSpan _gracePeriod = TimeSpan.FromMinutes(15);

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

            // Allow check-in within grace period before shift starts
            var earliestCheckIn = shift.StartTime - _gracePeriod;

            // For shifts that span midnight, we need to handle the time comparison differently
            bool isShiftSpanningMidnight = shift.EndTime < shift.StartTime;
            bool isTimeBeforeMidnight = timeOfDay <= TimeSpan.FromHours(24);
            bool isTimeAfterMidnight = timeOfDay >= TimeSpan.Zero;

            // Case 1: Normal shift (e.g., 9:00 - 17:00)
            if (!isShiftSpanningMidnight)
            {
                if (timeOfDay >= earliestCheckIn && timeOfDay <= shift.EndTime + _gracePeriod)
                {
                    return (true, null);
                }
            }
            // Case 2: Shift spanning midnight (e.g., 22:00 - 06:00)
            else
            {
                // Time is valid if it's between start time and midnight
                // OR between midnight and end time
                if ((timeOfDay >= earliestCheckIn && isTimeBeforeMidnight) ||
                    (isTimeAfterMidnight && timeOfDay <= shift.EndTime + _gracePeriod))
                {
                    return (true, null);
                }
            }

            return (true, $"Time entry is outside of your scheduled shift ({shift.StartTime:hh\\:mm} - {shift.EndTime:hh\\:mm}). Please contact your supervisor if you need to work outside your scheduled hours.");
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