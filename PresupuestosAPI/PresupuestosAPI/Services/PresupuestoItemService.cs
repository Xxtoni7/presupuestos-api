using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Models;
using PresupuestosAPI.DTOs.PresupuestoItem;

namespace PresupuestosAPI.Services
{
    public class PresupuestoItemService
    {
        private readonly AppDbContext _context;

        public PresupuestoItemService(AppDbContext context) 
        {
            _context = context;
        }

        private static PresupuestoItemResponseDto MapToPresupuestoItemResponseDto(PresupuestoItem item)
        {
            return new PresupuestoItemResponseDto
            {
                IdItem = item.IdItem,
                Description = item.Description,
                Materials = item.Materials,
                Labor = item.Labor,
                Quantity = item.Quantity,
                Subtotal = item.Subtotal,
                IdPresupuesto = item.IdPresupuesto,
            };
        }

        public async Task<List<PresupuestoItemResponseDto>> GetItemsByPresupuestoIdAsync(int presupuestoId)
        {
            var items = await _context.PresupuestoItems
                .Where(i => i.IdPresupuesto == presupuestoId)
                .ToListAsync();

            return items.Select(MapToPresupuestoItemResponseDto).ToList();
        }

        public async Task<PresupuestoItemResponseDto?> GetItemByIdAsync(int id)
        {
            var item = await _context.PresupuestoItems.FindAsync(id);
            if (item == null)
            {
                return null;
            }
            return MapToPresupuestoItemResponseDto(item);
        }

        public async Task<PresupuestoItemResponseDto> CreateItemAsync(CreatePresupuestoItemDto dto)
        {
            var item = new PresupuestoItem
            {
                Description = dto.Description,
                Materials = dto.Materials,
                Labor = dto.Labor,
                Quantity = dto.Quantity,
                IdPresupuesto = dto.IdPresupuesto,
                Subtotal = (dto.Materials + dto.Labor) * dto.Quantity
            };
            _context.PresupuestoItems.Add(item);
            await _context.SaveChangesAsync();

            var presupuesto = await _context.Presupuestos.FindAsync(item.IdPresupuesto);
            if (presupuesto != null)
            {
                presupuesto.Total = await _context.PresupuestoItems
                     .Where(i => i.IdPresupuesto == item.IdPresupuesto)
                     .Select(i => (decimal?)i.Subtotal)
                     .SumAsync() ?? 0;
                await _context.SaveChangesAsync();
            }
            return MapToPresupuestoItemResponseDto(item);
        }

        public async Task<PresupuestoItemResponseDto?> UpdateItemAsync(int id, UpdatePresupuestoItemDto dto)
        {
            var existingItem = await _context.PresupuestoItems.FindAsync(id);
            if (existingItem == null)
            {
                return null;
            }

            existingItem.Description = dto.Description;
            existingItem.Materials = dto.Materials;
            existingItem.Labor = dto.Labor;
            existingItem.Quantity = dto.Quantity;
            existingItem.Subtotal = (dto.Materials + dto.Labor) * dto.Quantity;
            await _context.SaveChangesAsync();

            var presupuesto = await _context.Presupuestos.FindAsync(existingItem.IdPresupuesto);
            if (presupuesto != null)
            {
                presupuesto.Total = await _context.PresupuestoItems
                     .Where(i => i.IdPresupuesto == existingItem.IdPresupuesto)
                     .Select(i => (decimal?)i.Subtotal)
                     .SumAsync() ?? 0;
                await _context.SaveChangesAsync();
            }
            return MapToPresupuestoItemResponseDto(existingItem);
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var item = await _context.PresupuestoItems.FindAsync(id);
            if (item == null)
            {
                return false;
            }
            var idPresupuesto = item.IdPresupuesto;

            _context.PresupuestoItems.Remove(item);
            await _context.SaveChangesAsync();

            var presupuesto = await _context.Presupuestos.FindAsync(idPresupuesto);
            if (presupuesto != null)
            {
                presupuesto.Total = await _context.PresupuestoItems
                    .Where(i => i.IdPresupuesto == idPresupuesto)
                    .Select(i => (decimal?)i.Subtotal)
                    .SumAsync() ?? 0;

                await _context.SaveChangesAsync();
            }
            return true;
        }
    }
}
