using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.Models
{
    public class Plan
    {
        [Key]
        public int IdPlan { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int MaxCompanies { get; set; }

        public int MaxPresupuestos { get; set; }

        public int MaxPdfExports { get; set; }

        [Required]
        [MaxLength(20)]
        public string PdfExportLimitPeriod { get; set; } = "Lifetime";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}