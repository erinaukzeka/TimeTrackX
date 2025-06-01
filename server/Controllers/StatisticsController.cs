using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.DTOs;
using System.Linq;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("employee-hours")]
        public async Task<ActionResult<object>> GetEmployeeHoursStatistics([FromQuery] string timeRange = "week")
        {
            try
            {
                var startDate = timeRange switch
                {
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddMonths(-1),
                    "year" => DateTime.UtcNow.AddYears(-1),
                    _ => DateTime.MinValue
                };

                var stats = await _context.Users
                    .Select(user => new
                    {
                        userId = user.Id,
                        userName = user.Username,
                        timeEntries = user.TimeEntries
                            .Where(t => t.StartTime >= startDate && t.EndTime != null)
                            .ToList(),
                        projects = user.TimeEntries
                            .Where(t => t.StartTime >= startDate)
                            .Select(t => t.ProjectId)
                            .Distinct()
                            .Count()
                    })
                    .ToListAsync();

                var employeeStats = stats.Select(s => new
                {
                    userId = s.userId,
                    userName = s.userName,
                    totalHours = s.timeEntries.Sum(t => (t.EndTime.Value - t.StartTime).TotalHours),
                    projectCount = s.projects,
                    averageHoursPerDay = s.timeEntries.Any() 
                        ? s.timeEntries.Sum(t => (t.EndTime.Value - t.StartTime).TotalHours) / 
                          (timeRange == "all" ? 
                            (DateTime.UtcNow - s.timeEntries.Min(t => t.StartTime)).TotalDays : 
                            (timeRange == "week" ? 7 : timeRange == "month" ? 30 : 365))
                        : 0
                });

                return Ok(new { success = true, stats = employeeStats });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error retrieving employee statistics" });
            }
        }
    }
} 