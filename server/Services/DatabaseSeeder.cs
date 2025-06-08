using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.Models;

namespace TimeTrackX.API.Services
{
    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");

                // Check if admin user exists
                var adminExists = await _context.Users
                    .AnyAsync(u => u.Username == "admin");

                _logger.LogInformation($"Admin user exists: {adminExists}");

                if (!adminExists)
                {
                    _logger.LogInformation("Creating admin user...");

                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@timetrackx.com",
                        FirstName = "Admin",
                        LastName = "User",
                        Role = "Admin",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    adminUser.SetPassword("Test@123");
                    
                    _logger.LogInformation($"Admin user object created with Username: {adminUser.Username}, Role: {adminUser.Role}");

                    _context.Users.Add(adminUser);
                    await _context.SaveChangesAsync();

                    // Verify the user was created
                    var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                    _logger.LogInformation($"Admin user created successfully. User exists in DB: {createdUser != null}");
                    if (createdUser != null)
                    {
                        _logger.LogInformation($"Created user details - ID: {createdUser.Id}, Username: {createdUser.Username}, Role: {createdUser.Role}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
} 