using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresupuestosAPI.Models
{
    public class Company
    {
        [Key]
        public int IdCompany{ get; set; }
        [Required]
        [MaxLength(60)]
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? ColorMain { get; set; }
        public string? ColorSecondary { get; set; }
        [MaxLength(80)]
        public string? Phone { get;set; }
        [MaxLength(80)]
        [EmailAddress]
        public string? Email { get; set; }
        [MaxLength(80)]
        public string? Address { get; set; }
        public string? Industry { get; set; }

        public int WorkspaceId { get; set; }

        [ForeignKey(nameof(WorkspaceId))]
        public Workspace? Workspace { get; set; }

        public List<Presupuesto>? Presupuestos { get; set; }
    }
}
