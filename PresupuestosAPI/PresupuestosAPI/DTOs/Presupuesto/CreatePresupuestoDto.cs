using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Presupuesto
{
    public class CreatePresupuestoDto
    {
        [Required]
        [MaxLength(60)]
        public string Title { get; set; } = string.Empty;
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
        public int IdCompany { get; set; }
    }
}
