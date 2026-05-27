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

            if (await context.Plans.AnyAsync())
            {
                return;
            }

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
                    Description = "Plan para profesionales y negocios chicos.",
                    Price = 14999,
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
                    Price = 20900,
                    MaxCompanies = -1,
                    MaxPresupuestos = -1,
                    MaxPdfExports = -1,
                    PdfExportLimitPeriod = "Unlimited",
                    IsActive = true
                }
            };

            context.Plans.AddRange(plans);
            await context.SaveChangesAsync();
        }
    }
}