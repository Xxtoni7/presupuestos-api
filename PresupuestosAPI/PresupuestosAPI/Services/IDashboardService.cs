using PresupuestosAPI.DTOs.Dashboard;

namespace PresupuestosAPI.Services
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetSummaryAsync();
    }
}