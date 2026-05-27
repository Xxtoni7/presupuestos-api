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
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? ColorMain { get; set; }
        public string? ColorSecondary { get; set; }
        [MaxLength(30)]
        public string? Phone { get;set; }
        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }
        [MaxLength(200)]
        public string? Address { get; set; }
        public string? Industry { get; set; }

        public int WorkspaceId { get; set; }

        [ForeignKey(nameof(WorkspaceId))]
        public Workspace? Workspace { get; set; }

        public List<Presupuesto>? Presupuestos { get; set; }
    }
}
