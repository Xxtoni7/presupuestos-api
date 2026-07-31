using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Auth
{
    public class ResendEmailConfirmationRequestDto
    {
        [Required(ErrorMessage = "El email es obligatorio.")]
        [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
        [MaxLength(256, ErrorMessage = "El email no puede superar los 256 caracteres.")]
        public string Email { get; set; } = string.Empty;
    }
}