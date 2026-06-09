using System;

namespace PresupuestosAPI.DTOs.Dashboard
{
    public class DashboardMetricsDto
    {
        public decimal TotalBudgeted { get; set; }
        public decimal AverageBudgetAmount { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalPresupuestos { get; set; }
        public int TotalPdfExportsUsed { get; set; }
        public DateTime? LastPresupuestoDate { get; set; }
        public decimal? HighestBudgetTotal { get; set; }
        public string? HighestBudgetClientName { get; set; }
        public string? HighestBudgetTitle { get; set; }
    }
}