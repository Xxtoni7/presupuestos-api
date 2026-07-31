using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Auth
{
    public class ResetPasswordRequestDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "El token es obligatorio.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [MaxLength(100, ErrorMessage = "La contraseña no puede superar los 100 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La confirmación de contraseña es obligatoria.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}