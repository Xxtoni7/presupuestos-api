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
            presupuesto.ClientNumber = updatePresupuesto.ClientNumber;
            presupuesto.FechaPresupuesto = updatePresupuesto.FechaPresupuesto;
            presupuesto.FechaVencimiento = updatePresupuesto.FechaVencimiento;

            await _context.SaveChangesAsync();
            return presupuesto;
        }

        public async Task<bool> DeletePresupuentoAsync(int id)
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
