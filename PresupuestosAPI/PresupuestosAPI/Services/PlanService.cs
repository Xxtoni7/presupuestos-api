using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.DTOs.Plan;

namespace PresupuestosAPI.Services
{
    public class PlanService
    {
        private readonly AppDbContext _context;

        public PlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AvailablePlanResponseDto>> GetAvailablePlansAsync()
        {
            var plans = await _context.Plans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .Select(p => new AvailablePlanResponseDto
                {
                    IdPlan = p.IdPlan,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    MaxCompanies = p.MaxCompanies,
                    MaxPresupuestos = p.MaxPresupuestos,
                    MaxPdfExports = p.MaxPdfExports,
                    PdfExportLimitPeriod = p.PdfExportLimitPeriod
                })
                .ToListAsync();

            return plans;
        }
    }
}