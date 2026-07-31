namespace PresupuestosAPI.DTOs.Dashboard
{
    public class DashboardPlanDto
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxCompanies { get; set; }
        public int MaxPresupuestos { get; set; }
        public int MaxPdfExports { get; set; }
        public string PdfExportLimitPeriod { get; set; } = string.Empty;
    }
}