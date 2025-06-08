using System.ComponentModel.DataAnnotations;

namespace TimeTrackX.API.DTOs
{
    public class RegisterUserDTO
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        public string Role { get; set; } = "Employee"; // Default role
    }

    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
    }

    public class UpdateUserDTO
    {
        [StringLength(50)]
        public string Username { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        public string Role { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
} 