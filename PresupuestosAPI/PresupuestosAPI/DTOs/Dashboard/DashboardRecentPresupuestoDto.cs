using System;

namespace PresupuestosAPI.DTOs.Dashboard
{
    public class DashboardRecentPresupuestoDto
    {
        public int IdPresupuesto { get; set; }
        public int IdCompany { get; set; }
        public string BudgetNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime FechaPresupuesto { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string? WorkAddress { get; set; }
    }
}