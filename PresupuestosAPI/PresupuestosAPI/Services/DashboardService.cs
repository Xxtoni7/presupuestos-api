using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.DTOs.Dashboard;

namespace PresupuestosAPI.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;
        private readonly PlanLimitService _planLimitService;

        public DashboardService( AppDbContext context, CurrentUserService currentUserService, PlanLimitService planLimitService )
        {
            _context = context;
            _currentUserService = currentUserService;
            _planLimitService = planLimitService;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync()
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var currentPlan = await _planLimitService.GetCurrentPlanUsageAsync();

            var companiesUsed = currentPlan.CompaniesUsed;
            var presupuestosUsed = currentPlan.PresupuestosUsed;
            var pdfExportsUsed = currentPlan.PdfExportsUsed;

            var companiesRemaining = CalculateRemaining(companiesUsed, currentPlan.MaxCompanies);
            var presupuestosRemaining = CalculateRemaining(presupuestosUsed, currentPlan.MaxPresupuestos);
            var pdfExportsRemaining = CalculateRemaining(pdfExportsUsed, currentPlan.MaxPdfExports);

            var companiesUsagePercentage = CalculateUsagePercentage(companiesUsed, currentPlan.MaxCompanies);
            var presupuestosUsagePercentage = CalculateUsagePercentage(presupuestosUsed, currentPlan.MaxPresupuestos);
            var pdfExportsUsagePercentage = CalculateUsagePercentage(pdfExportsUsed, currentPlan.MaxPdfExports);

            var presupuestosQuery = _context.Presupuestos
                .AsNoTracking()
                .Where(p => p.Company != null && p.Company.WorkspaceId == workspaceId);

            var totalBudgeted = await presupuestosQuery
                .SumAsync(p => (decimal?)p.Total) ?? 0;

            var averageBudgetAmount = presupuestosUsed > 0
                ? Math.Round(totalBudgeted / presupuestosUsed, 2)
                : 0;

            var lastPresupuestoDate = await presupuestosQuery
                .OrderByDescending(p => p.FechaPresupuesto)
                .ThenByDescending(p => p.IdPresupuesto)
                .Select(p => (DateTime?)p.FechaPresupuesto)
                .FirstOrDefaultAsync();

            var highestBudget = await presupuestosQuery
                .OrderByDescending(p => p.Total)
                .ThenByDescending(p => p.IdPresupuesto)
                .Select(p => new
                {
                    p.Total,
                    p.ClientName,
                    p.Title
                })
                .FirstOrDefaultAsync();

            var recentPresupuestos = await presupuestosQuery
                .OrderByDescending(p => p.FechaPresupuesto)
                .ThenByDescending(p => p.IdPresupuesto)
                .Take(5)
                .Select(p => new DashboardRecentPresupuestoDto
                {
                    IdPresupuesto = p.IdPresupuesto,
                    IdCompany = p.IdCompany,
                    BudgetNumber = p.BudgetNumber ?? string.Empty,
                    Title = p.Title,
                    ClientName = p.ClientName ?? string.Empty,
                    CompanyName = p.Company != null ? p.Company.Name : string.Empty,
                    Total = p.Total,
                    FechaPresupuesto = p.FechaPresupuesto,
                    FechaVencimiento = p.FechaVencimiento,
                    WorkAddress = p.WorkAddress
                })
                .ToListAsync();

            var firstCompany = await _context.Companies
                .AsNoTracking()
                .Where(c => c.WorkspaceId == workspaceId)
                .OrderBy(c => c.IdCompany)
                .Select(c => new
                {
                    c.IdCompany
                })
                .FirstOrDefaultAsync();

            var onboarding = BuildOnboarding(companiesUsed, presupuestosUsed, firstCompany?.IdCompany);

            var alerts = BuildAlerts(
                companiesUsed,
                presupuestosUsed,
                pdfExportsUsed,
                currentPlan.MaxCompanies,
                currentPlan.MaxPresupuestos,
                currentPlan.MaxPdfExports,
                companiesUsagePercentage,
                presupuestosUsagePercentage,
                pdfExportsUsagePercentage);

            return new DashboardSummaryDto
            {
                Plan = new DashboardPlanDto
                {
                    PlanName = currentPlan.PlanName,
                    Price = currentPlan.Price,
                    MaxCompanies = currentPlan.MaxCompanies,
                    MaxPresupuestos = currentPlan.MaxPresupuestos,
                    MaxPdfExports = currentPlan.MaxPdfExports,
                    PdfExportLimitPeriod = currentPlan.PdfExportLimitPeriod
                },

                Usage = new DashboardUsageDto
                {
                    CompaniesUsed = companiesUsed,
                    PresupuestosUsed = presupuestosUsed,
                    PdfExportsUsed = pdfExportsUsed,

                    CompaniesRemaining = companiesRemaining,
                    PresupuestosRemaining = presupuestosRemaining,
                    PdfExportsRemaining = pdfExportsRemaining,

                    CompaniesUsagePercentage = companiesUsagePercentage,
                    PresupuestosUsagePercentage = presupuestosUsagePercentage,
                    PdfExportsUsagePercentage = pdfExportsUsagePercentage,

                    PdfExportsPeriodStart = currentPlan.PdfExportsPeriodStart,
                    PdfExportsPeriodEnd = currentPlan.PdfExportsPeriodEnd
                },

                Metrics = new DashboardMetricsDto
                {
                    TotalBudgeted = totalBudgeted,
                    AverageBudgetAmount = averageBudgetAmount,
                    TotalCompanies = companiesUsed,
                    TotalPresupuestos = presupuestosUsed,
                    TotalPdfExportsUsed = pdfExportsUsed,
                    LastPresupuestoDate = lastPresupuestoDate,
                    HighestBudgetTotal = highestBudget?.Total,
                    HighestBudgetClientName = highestBudget?.ClientName,
                    HighestBudgetTitle = highestBudget?.Title
                },

                RecentPresupuestos = recentPresupuestos,

                Onboarding = onboarding,

                Alerts = alerts
            };
        }

        private static int? CalculateRemaining(int used, int max)
        {
            if (max == -1)
            {
                return null;
            }

            return Math.Max(max - used, 0);
        }

        private static decimal? CalculateUsagePercentage(int used, int max)
        {
            if (max == -1)
            {
                return null;
            }

            if (max <= 0)
            {
                return used > 0 ? 100 : 0;
            }

            var percentage = ((decimal)used / max) * 100;

            return Math.Min(Math.Round(percentage, 2), 100);
        }

        private static DashboardNextStepDto BuildOnboarding(
            int companiesUsed,
            int presupuestosUsed,
            int? firstCompanyId)
        {
            if (companiesUsed == 0)
            {
                return new DashboardNextStepDto
                {
                    Code = "CREATE_COMPANY",
                    Title = "Creá tu primera empresa",
                    Description = "Agregá los datos de tu negocio para empezar a generar presupuestos profesionales.",
                    ActionLabel = "Crear empresa",
                    ActionUrl = "/companies"
                };
            }

            if (presupuestosUsed == 0 && firstCompanyId.HasValue)
            {
                return new DashboardNextStepDto
                {
                    Code = "CREATE_PRESUPUESTO",
                    Title = "Creá tu primer presupuesto",
                    Description = "Ya tenés tu empresa cargada. Ahora podés crear un presupuesto profesional para tu cliente.",
                    ActionLabel = "Crear presupuesto",
                    ActionUrl = $"/companies/{firstCompanyId.Value}/budgets/new"
                };
            }

            return new DashboardNextStepDto
            {
                Code = "CONTINUE_WORKING",
                Title = "Continuá trabajando",
                Description = "Creá un nuevo presupuesto o revisá los últimos que cargaste.",
                ActionLabel = "Ver presupuestos",
                ActionUrl = "/budgets"
            };
        }

        private static List<DashboardAlertDto> BuildAlerts(
            int companiesUsed,
            int presupuestosUsed,
            int pdfExportsUsed,
            int maxCompanies,
            int maxPresupuestos,
            int maxPdfExports,
            decimal? companiesUsagePercentage,
            decimal? presupuestosUsagePercentage,
            decimal? pdfExportsUsagePercentage)
        {
            var alerts = new List<DashboardAlertDto>();

            if (companiesUsed == 0)
            {
                alerts.Add(new DashboardAlertDto
                {
                    Type = "info",
                    Title = "Configurá tu espacio",
                    Message = "Creá tu primera empresa para empezar a generar presupuestos.",
                    ActionLabel = "Crear empresa",
                    ActionUrl = "/companies"
                });
            }

            AddLimitAlert(
                alerts,
                used: companiesUsed,
                max: maxCompanies,
                usagePercentage: companiesUsagePercentage,
                warningTitle: "Estás cerca del límite de empresas",
                warningMessage: $"Usaste {companiesUsed} de {maxCompanies} empresas disponibles.",
                dangerTitle: "Límite de empresas alcanzado",
                dangerMessage: "Alcanzaste el límite de empresas de tu plan.");

            AddLimitAlert(
                alerts,
                used: presupuestosUsed,
                max: maxPresupuestos,
                usagePercentage: presupuestosUsagePercentage,
                warningTitle: "Estás cerca del límite de presupuestos",
                warningMessage: $"Usaste {presupuestosUsed} de {maxPresupuestos} presupuestos disponibles.",
                dangerTitle: "Límite de presupuestos alcanzado",
                dangerMessage: "Alcanzaste el límite de presupuestos de tu plan.");

            AddLimitAlert(
                alerts,
                used: pdfExportsUsed,
                max: maxPdfExports,
                usagePercentage: pdfExportsUsagePercentage,
                warningTitle: "Estás cerca del límite de exportaciones PDF",
                warningMessage: $"Usaste {pdfExportsUsed} de {maxPdfExports} exportaciones PDF disponibles.",
                dangerTitle: "Límite de PDFs alcanzado",
                dangerMessage: "Alcanzaste el límite de exportaciones PDF de tu plan.");

            return alerts
                .OrderByDescending(a => a.Type == "danger")
                .ThenByDescending(a => a.Type == "warning")
                .ToList();
        }

        private static void AddLimitAlert(
            List<DashboardAlertDto> alerts,
            int used,
            int max,
            decimal? usagePercentage,
            string warningTitle,
            string warningMessage,
            string dangerTitle,
            string dangerMessage)
        {
            if (max == -1)
            {
                return;
            }

            if (used >= max)
            {
                alerts.Add(new DashboardAlertDto
                {
                    Type = "danger",
                    Title = dangerTitle,
                    Message = dangerMessage,
                    ActionLabel = "Mejorar plan",
                    ActionUrl = "/settings"
                });

                return;
            }

            if (usagePercentage >= 80)
            {
                alerts.Add(new DashboardAlertDto
                {
                    Type = "warning",
                    Title = warningTitle,
                    Message = warningMessage,
                    ActionLabel = "Ver planes",
                    ActionUrl = "/settings"
                });
            }
        }
    }
}