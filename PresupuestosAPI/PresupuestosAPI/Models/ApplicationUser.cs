using Microsoft.AspNetCore.Identity;

namespace PresupuestosAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
