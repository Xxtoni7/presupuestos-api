using System.Security.Claims;

namespace PresupuestosAPI.Services
{
    public class CurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("Usuario no autenticado.");
            }

            return userId;
        }

        public int GetWorkspaceId()
        {
            var workspaceId = _httpContextAccessor.HttpContext?.User.FindFirstValue("workspaceId");

            if (string.IsNullOrWhiteSpace(workspaceId))
            {
                throw new UnauthorizedAccessException("Workspace no encontrado en el token.");
            }

            if (!int.TryParse(workspaceId, out var parsedWorkspaceId))
            {
                throw new UnauthorizedAccessException("Workspace inválido en el token.");
            }

            return parsedWorkspaceId;
        }
    }
}