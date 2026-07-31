namespace PresupuestosAPI.DTOs.Plan
{
    public class CurrentPlanResponseDto
    {
        public string PlanName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CompaniesUsed { get; set; }
        public int MaxCompanies { get; set; }
        public int PresupuestosUsed { get; set; }
        public int MaxPresupuestos { get; set; }
        public int PdfExportsUsed { get; set; }
        public int MaxPdfExports { get; set; }
        public string PdfExportLimitPeriod { get; set; } = string.Empty;
        public DateTime? PdfExportsPeriodStart { get; set; }
        public DateTime? PdfExportsPeriodEnd { get; set; }
    }
}