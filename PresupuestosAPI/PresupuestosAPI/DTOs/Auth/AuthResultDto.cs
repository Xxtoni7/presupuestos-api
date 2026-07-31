namespace PresupuestosAPI.DTOs.Auth
{
    public class AuthResultDto
    {
        public AuthResponseDto Response { get; set; } = new();
        public string RefreshToken { get; set; } = string.Empty;
    }
}