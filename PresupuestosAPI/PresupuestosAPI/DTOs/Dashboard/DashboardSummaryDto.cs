using System.Collections.Generic;

namespace PresupuestosAPI.DTOs.Dashboard
{
    public class DashboardSummaryDto
    {
        public DashboardPlanDto Plan { get; set; } = new();
        public DashboardUsageDto Usage { get; set; } = new();
        public DashboardMetricsDto Metrics { get; set; } = new();
        public List<DashboardRecentPresupuestoDto> RecentPresupuestos { get; set; } = new();
        public DashboardNextStepDto Onboarding { get; set; } = new();
        public List<DashboardAlertDto> Alerts { get; set; } = new();
    }
}