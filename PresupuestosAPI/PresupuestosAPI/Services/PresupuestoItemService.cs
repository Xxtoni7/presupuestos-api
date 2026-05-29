using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Models;
using PresupuestosAPI.DTOs.PresupuestoItem;

namespace PresupuestosAPI.Services
{
    public class PresupuestoItemService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public PresupuestoItemService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
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

        private async Task<Presupuesto?> GetOwnedPresupuestoAsync(int presupuestoId)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            return await _context.Presupuestos
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p =>
                    p.IdPresupuesto == presupuestoId &&
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);
        }

        private async Task RecalculatePresupuestoTotalAsync(int presupuestoId)
        {
            var presupuesto = await _context.Presupuestos.FindAsync(presupuestoId);

            if (presupuesto == null)
            {
                return;
            }

            presupuesto.Total = await _context.PresupuestoItems
                .Where(i => i.IdPresupuesto == presupuestoId)
                .Select(i => (decimal?)i.Subtotal)
                .SumAsync() ?? 0;

            await _context.SaveChangesAsync();
        }

        public async Task<List<PresupuestoItemResponseDto>> GetItemsByPresupuestoIdAsync(int presupuestoId)
        {
            var presupuesto = await GetOwnedPresupuestoAsync(presupuestoId);

            if (presupuesto == null)
            {
                return new List<PresupuestoItemResponseDto>();
            }

            var items = await _context.PresupuestoItems
                .Where(i => i.IdPresupuesto == presupuestoId)
                .ToListAsync();

            return items.Select(MapToPresupuestoItemResponseDto).ToList();
        }

        public async Task<PresupuestoItemResponseDto?> GetItemByIdAsync(int id)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var item = await _context.PresupuestoItems
                .Include(i => i.Presupuesto)
                    .ThenInclude(p => p!.Company)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == id &&
                    i.Presupuesto != null &&
                    i.Presupuesto.Company != null &&
                    i.Presupuesto.Company.WorkspaceId == workspaceId);

            if (item == null)
            {
                return null;
            }

            return MapToPresupuestoItemResponseDto(item);
        }

        public async Task<PresupuestoItemResponseDto> CreateItemAsync(CreatePresupuestoItemDto dto)
        {
            var presupuesto = await GetOwnedPresupuestoAsync(dto.IdPresupuesto);

            if (presupuesto == null)
            {
                throw new UnauthorizedAccessException("Presupuesto no encontrado.");
            }

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

            await RecalculatePresupuestoTotalAsync(item.IdPresupuesto);

            return MapToPresupuestoItemResponseDto(item);
        }

        public async Task<PresupuestoItemResponseDto?> UpdateItemAsync(int id, UpdatePresupuestoItemDto dto)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var existingItem = await _context.PresupuestoItems
                .Include(i => i.Presupuesto)
                    .ThenInclude(p => p!.Company)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == id &&
                    i.Presupuesto != null &&
                    i.Presupuesto.Company != null &&
                    i.Presupuesto.Company.WorkspaceId == workspaceId);

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

            await RecalculatePresupuestoTotalAsync(existingItem.IdPresupuesto);

            return MapToPresupuestoItemResponseDto(existingItem);
        }

        public async Task<bool> DeleteItemAsync(int id)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var item = await _context.PresupuestoItems
                .Include(i => i.Presupuesto)
                    .ThenInclude(p => p!.Company)
                .FirstOrDefaultAsync(i =>
                    i.IdItem == id &&
                    i.Presupuesto != null &&
                    i.Presupuesto.Company != null &&
                    i.Presupuesto.Company.WorkspaceId == workspaceId);

            if (item == null)
            {
                return false;
            }

            var idPresupuesto = item.IdPresupuesto;

            _context.PresupuestoItems.Remove(item);
            await _context.SaveChangesAsync();

            await RecalculatePresupuestoTotalAsync(idPresupuesto);

            return true;
        }
    }
}