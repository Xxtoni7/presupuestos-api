namespace PresupuestosAPI.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
        public int WorkspaceId { get; set; }
        public string PlanName { get; set; } = string.Empty;
    }
}