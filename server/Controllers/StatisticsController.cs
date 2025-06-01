using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.DTOs;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
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

        [HttpGet("checkin-trends")]
        public async Task<ActionResult<object>> GetCheckInOutTrends([FromQuery] string timeRange = "week", [FromQuery] string viewType = "daily")
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

                var timeEntries = await _context.TimeEntries
                    .Where(t => t.StartTime >= startDate)
                    .ToListAsync();

                if (!timeEntries.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        trends = new
                        {
                            checkInTrends = new List<object>(),
                            checkOutTrends = new List<object>()
                        }
                    });
                }

                var checkInTrends = new List<object>();
                var checkOutTrends = new List<object>();

                if (viewType == "hourly")
                {
                    // Initialize all hours with 0 count
                    checkInTrends = Enumerable.Range(0, 24)
                        .Select(hour => new { time = hour, count = 0 })
                        .ToList<object>();

                    checkOutTrends = Enumerable.Range(0, 24)
                        .Select(hour => new { time = hour, count = 0 })
                        .ToList<object>();

                    // Update counts from actual data
                    var checkInCounts = timeEntries
                        .GroupBy(t => t.StartTime.Hour)
                        .Select(g => new { time = g.Key, count = g.Count() });

                    var checkOutCounts = timeEntries
                        .Where(t => t.EndTime.HasValue)
                        .GroupBy(t => t.EndTime.Value.Hour)
                        .Select(g => new { time = g.Key, count = g.Count() });

                    checkInTrends = checkInTrends
                        .Cast<dynamic>()
                        .Select(x => new
                        {
                            time = x.time,
                            count = checkInCounts.FirstOrDefault(c => c.time == x.time)?.count ?? 0
                        })
                        .OrderBy(x => x.time)
                        .ToList<object>();

                    checkOutTrends = checkOutTrends
                        .Cast<dynamic>()
                        .Select(x => new
                        {
                            time = x.time,
                            count = checkOutCounts.FirstOrDefault(c => c.time == x.time)?.count ?? 0
                        })
                        .OrderBy(x => x.time)
                        .ToList<object>();
                }
                else
                {
                    // Initialize all days with 0 count
                    var daysOfWeek = Enum.GetValues<DayOfWeek>();
                    checkInTrends = daysOfWeek
                        .Select(day => new { time = day.ToString(), count = 0 })
                        .ToList<object>();

                    checkOutTrends = daysOfWeek
                        .Select(day => new { time = day.ToString(), count = 0 })
                        .ToList<object>();

                    // Update counts from actual data
                    var checkInCounts = timeEntries
                        .GroupBy(t => t.StartTime.DayOfWeek)
                        .Select(g => new { time = g.Key.ToString(), count = g.Count() });

                    var checkOutCounts = timeEntries
                        .Where(t => t.EndTime.HasValue)
                        .GroupBy(t => t.EndTime.Value.DayOfWeek)
                        .Select(g => new { time = g.Key.ToString(), count = g.Count() });

                    checkInTrends = checkInTrends
                        .Cast<dynamic>()
                        .Select(x => new
                        {
                            time = x.time,
                            count = checkInCounts.FirstOrDefault(c => c.time == x.time)?.count ?? 0
                        })
                        .OrderBy(x => Enum.Parse<DayOfWeek>(x.time))
                        .ToList<object>();

                    checkOutTrends = checkOutTrends
                        .Cast<dynamic>()
                        .Select(x => new
                        {
                            time = x.time,
                            count = checkOutCounts.FirstOrDefault(c => c.time == x.time)?.count ?? 0
                        })
                        .OrderBy(x => Enum.Parse<DayOfWeek>(x.time))
                        .ToList<object>();
                }

                return Ok(new
                {
                    success = true,
                    trends = new
                    {
                        checkInTrends = checkInTrends,
                        checkOutTrends = checkOutTrends
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error retrieving check-in/out trends" });
            }
        }
    }
} 