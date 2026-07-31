using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Auth
{
    public class GoogleLoginRequestDto
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }
}