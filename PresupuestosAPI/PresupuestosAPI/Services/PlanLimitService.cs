using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Exceptions;

namespace PresupuestosAPI.Services
{
    public class PlanLimitService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public PlanLimitService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task EnsureCanCreateCompanyAsync()
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s =>
                    s.WorkspaceId == workspaceId &&
                    s.Status == "Active");

            if (subscription == null || subscription.Plan == null)
            {
                throw new InvalidOperationException("No se encontró una suscripción activa.");
            }

            var maxCompanies = subscription.Plan.MaxCompanies;

            if (maxCompanies == -1)
            {
                return;
            }

            var currentCompanies = await _context.Companies
                .CountAsync(c => c.WorkspaceId == workspaceId);

            if (currentCompanies >= maxCompanies)
            {
                throw new PlanLimitExceededException("Alcanzaste el límite de empresas de tu plan.");
            }
        }

        public async Task EnsureCanCreatePresupuestoAsync()
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s =>
                    s.WorkspaceId == workspaceId &&
                    s.Status == "Active");

            if (subscription == null || subscription.Plan == null)
            {
                throw new InvalidOperationException("No se encontró una suscripción activa.");
            }

            var maxPresupuestos = subscription.Plan.MaxPresupuestos;

            if (maxPresupuestos == -1)
            {
                return;
            }

            var currentPresupuestos = await _context.Presupuestos
                .Include(p => p.Company)
                .CountAsync(p =>
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);

            if (currentPresupuestos >= maxPresupuestos)
            {
                throw new PlanLimitExceededException("Alcanzaste el límite de presupuestos de tu plan.");
            }
        }


    }
}