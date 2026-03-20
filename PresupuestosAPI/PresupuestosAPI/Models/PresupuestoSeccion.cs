using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresupuestosAPI.Models
{
    public class PresupuestoSeccion
    {
        [Key]
        public int IdSection { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int Order { get; set; }
        [Required]
        public string SectionType { get; set; }
        public int IdPresupuesto { get; set; }

        [ForeignKey(nameof(IdPresupuesto))]
        public Presupuesto Presupuesto { get; set; }

    }
}
