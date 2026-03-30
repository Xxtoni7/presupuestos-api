using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Services
{
    public class PresupuestoService
    {
        private readonly AppDbContext _context;
        public PresupuestoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Presupuesto>> GetAllPresupuestosAsync()
        {
            return await _context.Presupuestos.ToListAsync();
        }

        public async Task<List<Presupuesto>> GetPresupuestosByCompanyIdAsync(int companyId)
        {
            return await _context.Presupuestos
                .Where(p => p.IdCompany == companyId)
                .ToListAsync();
        }

        public async Task<Presupuesto?> GetPresupuestoByIdAsync(int id)
        {
            return await _context.Presupuestos.FindAsync(id);
        }

        public async Task<List<Presupuesto>> GetPresupuestosByTitleAsync(string title)
        {
            return await _context.Presupuestos.Where(p => p.Title.Contains(title)).ToListAsync();
        }

        public async Task<Presupuesto> CreatePresupuestoAsync(Presupuesto presupuesto)
        {
            var year = DateTime.Now.Year;

            var lastPresupuesto = await _context.Presupuestos
                .Where(p => p.BudgetNumber.StartsWith($"PRES-{year}"))
                .OrderByDescending(p => p.IdPresupuesto)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastPresupuesto != null)
            {
                var lastNumberPart = lastPresupuesto.BudgetNumber.Split('-').Last();
                nextNumber = int.Parse(lastNumberPart) + 1;
            }

            presupuesto.BudgetNumber = $"PRES-{year}-{nextNumber.ToString("D4")}";

            if (presupuesto.Items != null)
            {
                foreach (var item in presupuesto.Items)
                {
                    item.Subtotal = (item.Materials + item.Labor) * item.Quantity;
                }

                presupuesto.Total = presupuesto.Items.Sum(i => i.Subtotal);
            }
            else
            {
                presupuesto.Total = 0;
            }

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();
            return presupuesto;
        }

        public async Task<Presupuesto?> UpdatePresupuestoAsync(int id, Presupuesto updatePresupuesto)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);
            if (presupuesto == null)
            {
                return null;
            }

            presupuesto.Title = updatePresupuesto.Title;
            presupuesto.ClientName = updatePresupuesto.ClientName;
            presupuesto.FechaPresupuesto = updatePresupuesto.FechaPresupuesto;
            presupuesto.FechaVencimiento = updatePresupuesto.FechaVencimiento;
            presupuesto.WorkAddress = updatePresupuesto.WorkAddress;
            presupuesto.JobDescription = updatePresupuesto.JobDescription;
            presupuesto.EstimatedTime = updatePresupuesto.EstimatedTime;
            presupuesto.PaymentTerms = updatePresupuesto.PaymentTerms;
            presupuesto.Observations = updatePresupuesto.Observations;

            await _context.SaveChangesAsync();
            return presupuesto;
        }

        public async Task<bool> DeletePresupuestoAsync(int id)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(id);
            if (presupuesto == null)
            {
                return false;
            }

            _context.Presupuestos.Remove(presupuesto);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
