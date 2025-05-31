using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTrackX.API.Data;
using TimeTrackX.API.DTOs;
using TimeTrackX.API.Models;
using TimeTrackX.API.Services;

namespace TimeTrackX.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public UsersController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: api/Users/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDTO>> Register(RegisterUserDTO registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username is already taken");
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email is already registered");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Role = registerDto.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            user.SetPassword(registerDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                UserId = user.Id
            };
        }

        // POST: api/Users/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDTO>> Login(LoginDTO loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || !user.VerifyPassword(loginDto.Password))
            {
                return Unauthorized("Invalid username or password");
            }

            if (!user.IsActive)
            {
                return Unauthorized("Account is deactivated");
            }

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                UserId = user.Id
            };
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Include(u => u.AssignedProjects)
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.AssignedProjects)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Only allow users to access their own data unless they're an admin
            if (User.FindFirst("UserId")?.Value != id.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO updateDto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Only allow users to update their own data unless they're an admin
            if (User.FindFirst("UserId")?.Value != id.ToString() && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            // Only admins can update roles and active status
            if (!User.IsInRole("Admin"))
            {
                updateDto.Role = user.Role;
                updateDto.IsActive = user.IsActive;
            }

            if (!string.IsNullOrEmpty(updateDto.Username) && updateDto.Username != user.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == updateDto.Username))
                {
                    return BadRequest("Username is already taken");
                }
                user.Username = updateDto.Username;
            }

            if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != user.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == updateDto.Email))
                {
                    return BadRequest("Email is already registered");
                }
                user.Email = updateDto.Email;
            }

            if (!string.IsNullOrEmpty(updateDto.Password))
            {
                user.SetPassword(updateDto.Password);
            }

            if (!string.IsNullOrEmpty(updateDto.FirstName))
            {
                user.FirstName = updateDto.FirstName;
            }

            if (!string.IsNullOrEmpty(updateDto.LastName))
            {
                user.LastName = updateDto.LastName;
            }

            if (!string.IsNullOrEmpty(updateDto.Role))
            {
                user.Role = updateDto.Role;
            }

            if (updateDto.IsActive.HasValue)
            {
                user.IsActive = updateDto.IsActive.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
} 