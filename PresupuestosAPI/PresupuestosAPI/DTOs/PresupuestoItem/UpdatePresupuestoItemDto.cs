namespace PresupuestosAPI.DTOs.PresupuestoItem
{
    public class UpdatePresupuestoItemDto
    {
        public string? Description { get; set; }
        public decimal Materials { get; set; }
        public decimal Labor { get; set; }
        public int Quantity { get; set; }
    }
}
