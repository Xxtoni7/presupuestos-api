using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Services
{
    public class ItemService
    {
        private readonly AppDbContext _context;

        public ItemService(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<List<PresupuestoItem>> GetItemsByPresupuestoIdAsync(int presupuestoId)
        {
            return await _context.PresupuestoItems
                .Where(i => i.IdPresupuesto == presupuestoId)
                .ToListAsync();
        }

        public async Task<PresupuestoItem?> GetItemByIdAsync(int id)
        {
            return await _context.PresupuestoItems.FindAsync(id);
        }

        public async Task<PresupuestoItem> CreateItemAsync(PresupuestoItem item)
        {
            item.Subtotal = (item.Materials + item.Labor) * item.Quantity;
            _context.PresupuestoItems.Add(item);
            await _context.SaveChangesAsync();
            var presupuesto = await _context.Presupuestos.FindAsync(item.IdPresupuesto);
            if (presupuesto != null)
            {
                presupuesto.Total = await _context.PresupuestoItems
                    .Where(i => i.IdPresupuesto == item.IdPresupuesto)
                    .SumAsync(i => i.Subtotal);
                await _context.SaveChangesAsync();
            }
            return item;
        }

        public async Task<PresupuestoItem?> UpdateItemAsync(int id, PresupuestoItem item)
        {
            var existingItem = await _context.PresupuestoItems.FindAsync(id);
            if (existingItem == null)
            {
                return null;
            }

            existingItem.Description = item.Description;
            existingItem.Materials = item.Materials;
            existingItem.Labor = item.Labor;
            existingItem.Quantity = item.Quantity;
            existingItem.Subtotal = (item.Materials + item.Labor) * item.Quantity;
            await _context.SaveChangesAsync();

            var presupuesto = await _context.Presupuestos.FindAsync(existingItem.IdPresupuesto);
            if (presupuesto != null)
            {
                presupuesto.Total = await _context.PresupuestoItems
                    .Where(i => i.IdPresupuesto == existingItem.IdPresupuesto)
                    .SumAsync(i => i.Subtotal);
                await _context.SaveChangesAsync();
            }
            return existingItem;
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _context.PresupuestoItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }
            _context.PresupuestoItems.Remove(item);
            await _context.SaveChangesAsync();

            var presupuesto = await _context.Presupuestos.FindAsync(item.IdPresupuesto);
            if (presupuesto != null)
            {
                presupuesto.Total = await _context.PresupuestoItems
                    .Where(i => i.IdPresupuesto == item.IdPresupuesto)
                    .SumAsync(i => i.Subtotal);
                await _context.SaveChangesAsync();
            }
            return true;
        }

    }
}
