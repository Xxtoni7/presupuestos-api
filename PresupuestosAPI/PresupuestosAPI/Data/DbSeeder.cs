using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedPlansAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var plans = new List<Plan>
            {
                new Plan
                {
                    Name = "Free",
                    Description = "Plan gratuito para probar el sistema.",
                    Price = 0,
                    MaxCompanies = 1,
                    MaxPresupuestos = 3,
                    MaxPdfExports = 3,
                    PdfExportLimitPeriod = "Lifetime",
                    IsActive = true
                },
                new Plan
                {
                    Name = "Pro",
                    Description = "Plan para profesionales y negocios con poco volumen de trabajo.",
                    Price = 10000,
                    MaxCompanies = 5,
                    MaxPresupuestos = 50,
                    MaxPdfExports = 70,
                    PdfExportLimitPeriod = "Monthly",
                    IsActive = true
                },
                new Plan
                {
                    Name = "Business",
                    Description = "Plan para negocios con mayor volumen de trabajo.",
                    Price = 17000,
                    MaxCompanies = -1,
                    MaxPresupuestos = -1,
                    MaxPdfExports = -1,
                    PdfExportLimitPeriod = "Unlimited",
                    IsActive = true
                }
            };

            var hasChanges = false;

            foreach (var plan in plans)
            {
                var existingPlan = await context.Plans
                    .FirstOrDefaultAsync(p => p.Name == plan.Name);

                if (existingPlan == null)
                {
                    context.Plans.Add(plan);
                    hasChanges = true;
                    continue;
                }

                if (existingPlan.Description != plan.Description)
                {
                    existingPlan.Description = plan.Description;
                    hasChanges = true;
                }

                if (existingPlan.Price != plan.Price)
                {
                    existingPlan.Price = plan.Price;
                    hasChanges = true;
                }

                if (existingPlan.MaxCompanies != plan.MaxCompanies)
                {
                    existingPlan.MaxCompanies = plan.MaxCompanies;
                    hasChanges = true;
                }

                if (existingPlan.MaxPresupuestos != plan.MaxPresupuestos)
                {
                    existingPlan.MaxPresupuestos = plan.MaxPresupuestos;
                    hasChanges = true;
                }

                if (existingPlan.MaxPdfExports != plan.MaxPdfExports)
                {
                    existingPlan.MaxPdfExports = plan.MaxPdfExports;
                    hasChanges = true;
                }

                if (existingPlan.PdfExportLimitPeriod != plan.PdfExportLimitPeriod)
                {
                    existingPlan.PdfExportLimitPeriod = plan.PdfExportLimitPeriod;
                    hasChanges = true;
                }

                if (existingPlan.IsActive != plan.IsActive)
                {
                    existingPlan.IsActive = plan.IsActive;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync();
            }
        }
    }
}