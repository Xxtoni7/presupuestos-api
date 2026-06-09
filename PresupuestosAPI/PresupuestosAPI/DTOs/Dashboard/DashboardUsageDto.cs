using System;

namespace PresupuestosAPI.DTOs.Dashboard
{
    public class DashboardUsageDto
    {
        public int CompaniesUsed { get; set; }
        public int PresupuestosUsed { get; set; }
        public int PdfExportsUsed { get; set; }
        public int? CompaniesRemaining { get; set; }
        public int? PresupuestosRemaining { get; set; }
        public int? PdfExportsRemaining { get; set; }
        public decimal? CompaniesUsagePercentage { get; set; }
        public decimal? PresupuestosUsagePercentage { get; set; }
        public decimal? PdfExportsUsagePercentage { get; set; }
        public DateTime? PdfExportsPeriodStart { get; set; }
        public DateTime? PdfExportsPeriodEnd { get; set; }
    }
}