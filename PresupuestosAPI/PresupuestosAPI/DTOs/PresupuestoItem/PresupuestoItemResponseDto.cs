namespace PresupuestosAPI.DTOs.PresupuestoItem
{
    public class PresupuestoItemResponseDto
    {
        public int IdItem { get; set; }
        public string? Description { get; set; }
        public decimal Materials { get; set; }
        public decimal Labor { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public int IdPresupuesto { get; set; }
    }
}
