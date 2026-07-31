using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresupuestosAPI.Models
{
    public class Subscription
    {
        [Key]
        public int IdSubscription { get; set; }

        public int WorkspaceId { get; set; }

        [ForeignKey(nameof(WorkspaceId))]
        public Workspace? Workspace { get; set; }

        public int PlanId { get; set; }

        [ForeignKey(nameof(PlanId))]
        public Plan? Plan { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "Active";

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }

        [MaxLength(150)]
        public string? MercadoPagoSubscriptionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}