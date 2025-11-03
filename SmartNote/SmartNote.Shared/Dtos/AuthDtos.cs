using System.ComponentModel.DataAnnotations;

namespace SmartNote.Shared.Dtos
{
    public class RegisterRequest
    {
        [Required, StringLength(32, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(64, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required, StringLength(32, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(64, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int ExpiresInSeconds { get; set; }
    }
}
