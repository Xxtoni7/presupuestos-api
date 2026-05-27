using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresupuestosAPI.Models
{
    public class WorkspaceUsage
    {
        [Key]
        public int IdWorkspaceUsage { get; set; }

        public int WorkspaceId { get; set; }

        [ForeignKey(nameof(WorkspaceId))]
        public Workspace? Workspace { get; set; }

        public int PdfExportsUsed { get; set; } = 0;

        public DateTime PdfExportsPeriodStart { get; set; } = DateTime.UtcNow;

        public DateTime? PdfExportsPeriodEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}