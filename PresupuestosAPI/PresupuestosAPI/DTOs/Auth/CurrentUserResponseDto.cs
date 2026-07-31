namespace PresupuestosAPI.DTOs.Auth
{
    public class CurrentUserResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
    }
}