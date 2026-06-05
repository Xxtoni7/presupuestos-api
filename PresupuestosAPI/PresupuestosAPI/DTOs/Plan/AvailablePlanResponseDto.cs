namespace PresupuestosAPI.DTOs.Plan
{
    public class AvailablePlanResponseDto
    {
        public int IdPlan { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int MaxCompanies { get; set; }

        public int MaxPresupuestos { get; set; }

        public int MaxPdfExports { get; set; }

        public string PdfExportLimitPeriod { get; set; } = string.Empty;
    }
}