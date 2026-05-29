namespace PresupuestosAPI.DTOs.Presupuesto
{
    public class PresupuestoResponseDto
    {
        public int IdPresupuesto { get; set; }
        public string Title { get; set; } = string.Empty;
        public string BudgetNumber { get; set; } = string.Empty;
        public string? ClientName { get; set; }
        public DateTime FechaPresupuesto { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? WorkAddress { get; set; }
        public string? JobDescription { get; set; }
        public string? EstimatedTime { get; set; }
        public string? PaymentTerms { get; set; }
        public string? Observations { get; set; }
        public decimal Total { get; set; }
        public int IdCompany { get; set; }
    }
}
