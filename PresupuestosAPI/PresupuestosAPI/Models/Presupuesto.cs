using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresupuestosAPI.Models
{
    public class Presupuesto
    {
        [Key]
        public int IdPresupuesto { get; set; }
        [Required]
        [MaxLength(60)]
        public string Title { get; set; } = string.Empty;
        public string? BudgetNumber { get; set; }
        [MaxLength(80)]
        public string? ClientName { get; set; }
        public DateTime FechaPresupuesto { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        [MaxLength(80)]
        public string? WorkAddress { get; set; }
        public string? JobDescription { get; set; }
        public string? EstimatedTime { get; set; }
        public string? PaymentTerms { get; set; }
        public string? Observations { get; set; }
        public decimal Total { get; set; }

        public int IdCompany { get; set; }

        [ForeignKey(nameof(IdCompany))]
        public Company? Company { get; set; }

        public List<PresupuestoItem>? Items { get; set; }
    }
}
