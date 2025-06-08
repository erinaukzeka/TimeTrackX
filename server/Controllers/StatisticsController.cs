using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.DTOs;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeSpan _lateThreshold = TimeSpan.FromHours(9); // 9:00 AM
        private readonly TimeSpan _absenceThreshold = TimeSpan.FromHours(12); // 12:00 PM
        private readonly string[] _validTimeRanges = new[] { "week", "month", "year" };

        public class StatisticsResponse
        {
            public int TotalUsers { get; set; }
            public int ActiveUsers { get; set; }
            public int TotalProjects { get; set; }
            public int ActiveProjects { get; set; }
            public int TotalTasks { get; set; }
            public Dictionary<string, int> TasksByStatus { get; set; }
            public Dictionary<string, double> AverageTimePerProject { get; set; }
            public List<UserProductivityStats> TopUsersByHours { get; set; }
            public Dictionary<string, int> ShiftDistribution { get; set; }
        }

        public class UserProductivityStats
        {
            public string Username { get; set; }
            public double TotalHours { get; set; }
            public int CompletedTasks { get; set; }
        }

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

        [HttpGet("attendance")]
        public async Task<ActionResult<object>> GetAttendanceStatistics([FromQuery] string timeRange = "month")
        {
            try
            {
                // Validate time range
                if (!_validTimeRanges.Contains(timeRange.ToLower()))
                {
                    return BadRequest(new { success = false, error = "Invalid time range. Valid values are: week, month, year" });
                }

                var startDate = timeRange.ToLower() switch
                {
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddMonths(-1),
                    "year" => DateTime.UtcNow.AddYears(-1),
                    _ => throw new ArgumentException("Invalid time range")
                };

                // Validate if we have any users
                if (!await _context.Users.AnyAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        attendance = new
                        {
                            lateArrivals = new List<object>(),
                            absences = new List<object>(),
                            summaryByEmployee = new List<object>()
                        }
                    });
                }

                var timeEntries = await _context.TimeEntries
                    .Include(t => t.User)
                    .Where(t => t.StartTime >= startDate)
                    .ToListAsync();

                // Group entries by date for trends
                var lateArrivals = timeEntries
                    .Where(t => t.StartTime.TimeOfDay > _lateThreshold)
                    .GroupBy(t => t.StartTime.Date)
                    .Select(g => new { date = g.Key.ToString("MM/dd"), count = g.Count() })
                    .OrderBy(x => DateTime.Parse(x.date))
                    .ToList();

                // Calculate absences (no time entries for a workday)
                var workDays = GetWorkdaysBetween(startDate, DateTime.UtcNow);
                
                if (!workDays.Any())
                {
                    return Ok(new
                    {
                        success = true,
                        attendance = new
                        {
                            lateArrivals = new List<object>(),
                            absences = new List<object>(),
                            summaryByEmployee = new List<object>()
                        }
                    });
                }

                var employeeEntries = timeEntries
                    .GroupBy(t => new { t.UserId, Date = t.StartTime.Date })
                    .Select(g => new { g.Key.UserId, g.Key.Date })
                    .ToList();

                var absences = workDays
                    .SelectMany(date => _context.Users.Select(u => new { UserId = u.Id, Date = date }))
                    .Where(x => !employeeEntries.Any(e => e.UserId == x.UserId && e.Date == x.Date))
                    .GroupBy(x => x.Date)
                    .Select(g => new { date = g.Key.ToString("MM/dd"), count = g.Count() })
                    .OrderBy(x => DateTime.Parse(x.date))
                    .ToList();

                // Calculate summary by employee
                var employeeSummary = await _context.Users
                    .Select(user => new
                    {
                        userId = user.Id,
                        userName = user.Username,
                        timeEntries = timeEntries.Where(t => t.UserId == user.Id).ToList()
                    })
                    .ToListAsync();

                var summaryByEmployee = employeeSummary.Select(e =>
                {
                    var totalWorkDays = workDays.Count;
                    var daysPresent = e.timeEntries
                        .Select(t => t.StartTime.Date)
                        .Distinct()
                        .Count();
                    var lateCount = e.timeEntries.Count(t => t.StartTime.TimeOfDay > _lateThreshold);
                    var absenceCount = totalWorkDays - daysPresent;
                    var attendanceRate = totalWorkDays > 0 ? ((double)daysPresent / totalWorkDays) * 100 : 0;

                    return new
                    {
                        userId = e.userId,
                        userName = e.userName,
                        lateCount = lateCount,
                        absenceCount = absenceCount,
                        attendanceRate = Math.Round(attendanceRate, 1),
                        status = GetAttendanceStatus(attendanceRate, lateCount)
                    };
                }).ToList();

                return Ok(new
                {
                    success = true,
                    attendance = new
                    {
                        lateArrivals = lateArrivals,
                        absences = absences,
                        summaryByEmployee = summaryByEmployee
                    }
                });
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { success = false, error = "An error occurred while retrieving attendance statistics" });
            }
        }

        [HttpGet("active-employees")]
        public async Task<ActionResult<object>> GetActiveEmployeesStatistics([FromQuery] string timeRange = "month")
        {
            try
            {
                if (!_validTimeRanges.Contains(timeRange.ToLower()))
                {
                    return BadRequest(new { success = false, error = "Invalid time range. Valid values are: week, month, year" });
                }

                var startDate = timeRange.ToLower() switch
                {
                    "week" => DateTime.UtcNow.AddDays(-7),
                    "month" => DateTime.UtcNow.AddMonths(-1),
                    "year" => DateTime.UtcNow.AddYears(-1),
                    _ => throw new ArgumentException("Invalid time range")
                };

                // Get all time entries within the time range
                var timeEntries = await _context.TimeEntries
                    .Include(t => t.User)
                    .Include(t => t.Project)
                    .Where(t => t.StartTime >= startDate && t.EndTime != null)
                    .ToListAsync();

                // Calculate top employees based on hours worked
                var topEmployees = timeEntries
                    .GroupBy(t => new { t.UserId, t.User.Username })
                    .Select(g => new
                    {
                        userId = g.Key.UserId,
                        userName = g.Key.Username,
                        hoursWorked = g.Sum(t => (t.EndTime.Value - t.StartTime).TotalHours),
                        projectCount = g.Select(t => t.ProjectId).Distinct().Count(),
                        completedTasks = g.Count()
                    })
                    .OrderByDescending(e => e.hoursWorked)
                    .Take(6)
                    .ToList();

                // Calculate project distribution
                var projectDistribution = timeEntries
                    .GroupBy(t => t.Project.Name)
                    .Select(g => new
                    {
                        name = g.Key,
                        value = g.Count()
                    })
                    .OrderByDescending(p => p.value)
                    .Take(6)
                    .ToList();

                // Calculate task completion rates
                var taskCompletion = timeEntries
                    .GroupBy(t => new { t.UserId, t.User.Username })
                    .Select(g =>
                    {
                        var totalTasks = g.Count();
                        var completedTasks = g.Count(t => t.EndTime != null);
                        return new
                        {
                            userId = g.Key.UserId,
                            userName = g.Key.Username,
                            completedTasks = completedTasks,
                            completionRate = Math.Round((double)completedTasks / totalTasks * 100, 1)
                        };
                    })
                    .OrderByDescending(t => t.completionRate)
                    .Take(5)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        topEmployees = topEmployees,
                        projectDistribution = projectDistribution,
                        taskCompletion = taskCompletion
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error retrieving active employees statistics" });
            }
        }

        [HttpGet]
        public async Task<ActionResult<StatisticsResponse>> GetStatistics()
        {
            var stats = new StatisticsResponse
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.Where(u => u.IsActive).CountAsync(),
                TotalProjects = await _context.Projects.CountAsync(),
                ActiveProjects = await _context.Projects.Where(p => p.IsActive).CountAsync(),
                TotalTasks = await _context.Tasks.CountAsync(),
                TasksByStatus = await _context.Tasks
                    .GroupBy(t => t.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count),
                AverageTimePerProject = await _context.TimeEntries
                    .Where(te => te.EndTime != null)
                    .GroupBy(te => te.ProjectId)
                    .Select(g => new
                    {
                        ProjectId = g.Key,
                        AverageHours = g.Average(te => 
                            ((DateTime)te.EndTime - te.StartTime).TotalHours)
                    })
                    .Join(_context.Projects,
                        avg => avg.ProjectId,
                        proj => proj.Id,
                        (avg, proj) => new { proj.Name, avg.AverageHours })
                    .ToDictionaryAsync(x => x.Name, x => x.AverageHours),
                TopUsersByHours = await _context.TimeEntries
                    .Where(te => te.EndTime != null)
                    .GroupBy(te => te.UserId)
                    .Select(g => new UserProductivityStats
                    {
                        Username = g.Select(te => te.User.Username).FirstOrDefault(),
                        TotalHours = g.Sum(te => 
                            ((DateTime)te.EndTime - te.StartTime).TotalHours),
                        CompletedTasks = _context.Tasks
                            .Count(t => t.AssignedUserId == g.Key && t.Status == "Completed")
                    })
                    .OrderByDescending(x => x.TotalHours)
                    .Take(5)
                    .ToListAsync(),
                ShiftDistribution = await _context.Shifts
                    .GroupBy(s => s.Type)
                    .Select(g => new { ShiftType = g.Key.ToString(), Count = g.Count() })
                    .ToDictionaryAsync(x => x.ShiftType, x => x.Count)
            };

            return Ok(stats);
        }

        private List<DateTime> GetWorkdaysBetween(DateTime start, DateTime end)
        {
            var workDays = new List<DateTime>();
            var current = start.Date;
            while (current <= end.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                {
                    workDays.Add(current);
                }
                current = current.AddDays(1);
            }
            return workDays;
        }

        private string GetAttendanceStatus(double attendanceRate, int lateCount)
        {
            if (attendanceRate >= 95 && lateCount <= 1)
                return "Excellent";
            if (attendanceRate >= 90 && lateCount <= 3)
                return "Good";
            if (attendanceRate >= 85 && lateCount <= 5)
                return "Fair";
            return "Poor";
        }
    }
} 