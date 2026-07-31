using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Exceptions;
using PresupuestosAPI.Models;
using PresupuestosAPI.DTOs.Plan;

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

        private static (DateTime PeriodStart, DateTime PeriodEnd) GetCurrentMonthlyPeriod(DateTime subscriptionStartDate, DateTime now)
        {
            var periodStart = subscriptionStartDate;
            var periodEnd = periodStart.AddMonths(1);

            while (periodEnd <= now)
            {
                periodStart = periodEnd;
                periodEnd = periodStart.AddMonths(1);
            }

            return (periodStart, periodEnd);
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

        public async Task ConsumePdfExportAsync(int presupuestoId)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuestoExists = await _context.Presupuestos
                .Include(p => p.Company)
                .AnyAsync(p =>
                    p.IdPresupuesto == presupuestoId &&
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);

            if (!presupuestoExists)
            {
                throw new UnauthorizedAccessException("Presupuesto no encontrado.");
            }

            var subscription = await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s =>
                    s.WorkspaceId == workspaceId &&
                    s.Status == "Active");

            if (subscription == null || subscription.Plan == null)
            {
                throw new InvalidOperationException("No se encontró una suscripción activa.");
            }

            var plan = subscription.Plan;

            if (plan.MaxPdfExports == -1 || plan.PdfExportLimitPeriod == "Unlimited")
            {
                return;
            }

            var usage = await _context.WorkspaceUsages
                .FirstOrDefaultAsync(u => u.WorkspaceId == workspaceId);

            if (usage == null)
            {
                usage = new WorkspaceUsage
                {
                    WorkspaceId = workspaceId,
                    PdfExportsUsed = 0,
                    PdfExportsPeriodStart = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WorkspaceUsages.Add(usage);
                await _context.SaveChangesAsync();
            }

            if (plan.PdfExportLimitPeriod == "Monthly")
            {
                var now = DateTime.UtcNow;
                var currentPeriod = GetCurrentMonthlyPeriod(subscription.StartDate, now);

                if (usage.PdfExportsPeriodStart < currentPeriod.PeriodStart ||
                    usage.PdfExportsPeriodEnd != currentPeriod.PeriodEnd)
                {
                    usage.PdfExportsUsed = 0;
                    usage.PdfExportsPeriodStart = currentPeriod.PeriodStart;
                    usage.PdfExportsPeriodEnd = currentPeriod.PeriodEnd;
                    usage.UpdatedAt = DateTime.UtcNow;
                }
            }

            if (usage.PdfExportsUsed >= plan.MaxPdfExports)
            {
                throw new PlanLimitExceededException("Alcanzaste el límite de exportaciones PDF de tu plan.");
            }

            usage.PdfExportsUsed += 1;
            usage.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<CurrentPlanResponseDto> GetCurrentPlanUsageAsync()
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

            var plan = subscription.Plan;

            var companiesUsed = await _context.Companies
                .CountAsync(c => c.WorkspaceId == workspaceId);

            var presupuestosUsed = await _context.Presupuestos
                .Include(p => p.Company)
                .CountAsync(p =>
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);

            var usage = await _context.WorkspaceUsages
                .FirstOrDefaultAsync(u => u.WorkspaceId == workspaceId);

            if (usage == null)
            {
                usage = new WorkspaceUsage
                {
                    WorkspaceId = workspaceId,
                    PdfExportsUsed = 0,
                    PdfExportsPeriodStart = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };

                _context.WorkspaceUsages.Add(usage);
                await _context.SaveChangesAsync();
            }

            if (plan.PdfExportLimitPeriod == "Monthly")
            {
                var now = DateTime.UtcNow;
                var currentPeriod = GetCurrentMonthlyPeriod(subscription.StartDate, now);

                if (usage.PdfExportsPeriodStart < currentPeriod.PeriodStart ||
                    usage.PdfExportsPeriodEnd != currentPeriod.PeriodEnd)
                {
                    usage.PdfExportsUsed = 0;
                    usage.PdfExportsPeriodStart = currentPeriod.PeriodStart;
                    usage.PdfExportsPeriodEnd = currentPeriod.PeriodEnd;
                    usage.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
            }

            return new CurrentPlanResponseDto
            {
                PlanName = plan.Name,
                Price = plan.Price,
                CompaniesUsed = companiesUsed,
                MaxCompanies = plan.MaxCompanies,
                PresupuestosUsed = presupuestosUsed,
                MaxPresupuestos = plan.MaxPresupuestos,
                PdfExportsUsed = usage.PdfExportsUsed,
                MaxPdfExports = plan.MaxPdfExports,
                PdfExportLimitPeriod = plan.PdfExportLimitPeriod,
                PdfExportsPeriodStart = usage.PdfExportsPeriodStart,
                PdfExportsPeriodEnd = usage.PdfExportsPeriodEnd
            };
        }
    }
}