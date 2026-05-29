using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.DTOs.Presupuesto;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Services
{
    public class PresupuestoService
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUserService;

        public PresupuestoService(AppDbContext context, CurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        private static PresupuestoResponseDto MapToPresupuestoResponseDto(Presupuesto presupuesto)
        {
            return new PresupuestoResponseDto
            {
                IdPresupuesto = presupuesto.IdPresupuesto,
                Title = presupuesto.Title,
                BudgetNumber = presupuesto.BudgetNumber!,
                ClientName = presupuesto.ClientName,
                FechaPresupuesto = presupuesto.FechaPresupuesto,
                FechaVencimiento = presupuesto.FechaVencimiento,
                WorkAddress = presupuesto.WorkAddress,
                JobDescription = presupuesto.JobDescription,
                EstimatedTime = presupuesto.EstimatedTime,
                PaymentTerms = presupuesto.PaymentTerms,
                Observations = presupuesto.Observations,
                Total = presupuesto.Total,
                IdCompany = presupuesto.IdCompany
            };
        }

        public async Task<List<PresupuestoResponseDto>> GetAllPresupuestosAsync()
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuestos = await _context.Presupuestos
                .Include(p => p.Company)
                .Where(p => p.Company != null && p.Company.WorkspaceId == workspaceId)
                .OrderByDescending(p => p.FechaPresupuesto)
                .ToListAsync();

            return presupuestos.Select(MapToPresupuestoResponseDto).ToList();
        }

        public async Task<List<PresupuestoResponseDto>> GetPresupuestosByCompanyIdAsync(int companyId)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuestos = await _context.Presupuestos
                .Include(p => p.Company)
                .Where(p =>
                    p.IdCompany == companyId &&
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId)
                .OrderByDescending(p => p.FechaPresupuesto)
                .ToListAsync();

            return presupuestos.Select(MapToPresupuestoResponseDto).ToList();
        }

        public async Task<PresupuestoResponseDto?> GetPresupuestoByIdAsync(int id)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuesto = await _context.Presupuestos
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p =>
                    p.IdPresupuesto == id &&
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);

            if (presupuesto == null)
            {
                return null;
            }

            return MapToPresupuestoResponseDto(presupuesto);
        }

        public async Task<List<PresupuestoResponseDto>> GetPresupuestosByTitleAsync(string title)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuestos = await _context.Presupuestos
                .Include(p => p.Company)
                .Where(p =>
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId &&
                    p.Title.Contains(title))
                .OrderByDescending(p => p.FechaPresupuesto)
                .ToListAsync();

            return presupuestos.Select(MapToPresupuestoResponseDto).ToList();
        }

        public async Task<PresupuestoResponseDto> CreatePresupuestoAsync(CreatePresupuestoDto dto)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var companyExists = await _context.Companies
                .AnyAsync(c => c.IdCompany == dto.IdCompany && c.WorkspaceId == workspaceId);

            if (!companyExists)
            {
                throw new UnauthorizedAccessException("La empresa no existe o no pertenece al usuario.");
            }

            var year = DateTime.Now.Year;

            var lastPresupuesto = await _context.Presupuestos
                .Where(p => p.BudgetNumber != null && p.BudgetNumber.StartsWith($"PRES-{year}"))
                .OrderByDescending(p => p.IdPresupuesto)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastPresupuesto != null)
            {
                var lastNumberPart = lastPresupuesto.BudgetNumber!.Split('-').Last();
                nextNumber = int.Parse(lastNumberPart) + 1;
            }

            var presupuesto = new Presupuesto
            {
                Title = dto.Title,
                BudgetNumber = $"PRES-{year}-{nextNumber.ToString("D4")}",
                ClientName = dto.ClientName,
                FechaPresupuesto = dto.FechaPresupuesto,
                FechaVencimiento = dto.FechaVencimiento,
                WorkAddress = dto.WorkAddress,
                JobDescription = dto.JobDescription,
                EstimatedTime = dto.EstimatedTime,
                PaymentTerms = dto.PaymentTerms,
                Observations = dto.Observations,
                Total = 0,
                IdCompany = dto.IdCompany
            };

            _context.Presupuestos.Add(presupuesto);
            await _context.SaveChangesAsync();

            return MapToPresupuestoResponseDto(presupuesto);
        }

        public async Task<PresupuestoResponseDto?> UpdatePresupuestoAsync(int id, UpdatePresupuestoDto dto)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuesto = await _context.Presupuestos
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p =>
                    p.IdPresupuesto == id &&
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);

            if (presupuesto == null)
            {
                return null;
            }

            presupuesto.Title = dto.Title;
            presupuesto.ClientName = dto.ClientName;
            presupuesto.FechaPresupuesto = dto.FechaPresupuesto;
            presupuesto.FechaVencimiento = dto.FechaVencimiento;
            presupuesto.WorkAddress = dto.WorkAddress;
            presupuesto.JobDescription = dto.JobDescription;
            presupuesto.EstimatedTime = dto.EstimatedTime;
            presupuesto.PaymentTerms = dto.PaymentTerms;
            presupuesto.Observations = dto.Observations;

            await _context.SaveChangesAsync();

            return MapToPresupuestoResponseDto(presupuesto);
        }

        public async Task<bool> DeletePresupuestoAsync(int id)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var presupuesto = await _context.Presupuestos
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p =>
                    p.IdPresupuesto == id &&
                    p.Company != null &&
                    p.Company.WorkspaceId == workspaceId);

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