using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresupuestosAPI.Models
{
    public class PresupuestoItem
    {
        [Key]
        public int IdItem { get; set; }
        [MaxLength(300)]
        public string? Description { get; set; }
        public decimal Materials { get; set; }
        public decimal Labor { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public int IdPresupuesto { get; set; }

        [ForeignKey(nameof(IdPresupuesto))]
        public Presupuesto? Presupuesto { get; set; }
    }
}
