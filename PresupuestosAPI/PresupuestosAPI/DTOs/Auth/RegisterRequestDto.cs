using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(80)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(8)]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}