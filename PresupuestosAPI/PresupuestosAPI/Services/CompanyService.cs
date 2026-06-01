using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.DTOs.Company;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Services
{
    public class CompanyService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        private readonly CurrentUserService _currentUserService;
        private readonly PlanLimitService _planLimitService;
        public CompanyService(
            AppDbContext context,
            CloudinaryService cloudinaryService,
            CurrentUserService currentUserService,
            PlanLimitService planLimitService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _currentUserService = currentUserService;
            _planLimitService = planLimitService;
        }

        private static CompanyResponseDto MapToCompanyResponseDto(Company company)
        {
            return new CompanyResponseDto
            {
                IdCompany = company.IdCompany,
                Name = company.Name,
                LogoUrl = company.LogoUrl,
                ColorMain = company.ColorMain,
                ColorSecondary = company.ColorSecondary,
                Phone = company.Phone,
                Email = company.Email,
                Address = company.Address,
                Industry = company.Industry
            };
        }

        public async Task<List<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var companies = await _context.Companies
                .Where(c => c.WorkspaceId == workspaceId)
                .OrderByDescending(c => c.IdCompany)
                .ToListAsync();

            return companies.Select(MapToCompanyResponseDto).ToList();
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int id)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.IdCompany == id && c.WorkspaceId == workspaceId);

            return company == null ? null : MapToCompanyResponseDto(company);
        }

        public async Task<List<CompanyResponseDto>> GetCompaniesByNameAsync(string name)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var companies = await _context.Companies
                .Where(c => c.WorkspaceId == workspaceId && c.Name.Contains(name))
                .OrderByDescending(c => c.IdCompany)
                .ToListAsync();

            return companies.Select(MapToCompanyResponseDto).ToList();
        }

        public async Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto dto)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            await _planLimitService.EnsureCanCreateCompanyAsync();

            var company = new Company
            {
                Name = dto.Name,
                LogoUrl = dto.LogoUrl,
                ColorMain = dto.ColorMain,
                ColorSecondary = dto.ColorSecondary,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                Industry = dto.Industry,
                WorkspaceId = workspaceId
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return MapToCompanyResponseDto(company);
        }

        public async Task<CompanyResponseDto?> UpdateCompanyAsync(int id, UpdateCompanyDto dto)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.IdCompany == id && c.WorkspaceId == workspaceId);

            if (company == null)
            {
                return null;
            }

            company.Name = dto.Name;
            company.LogoUrl = dto.LogoUrl;
            company.ColorMain = dto.ColorMain;
            company.ColorSecondary = dto.ColorSecondary;
            company.Phone = dto.Phone;
            company.Email = dto.Email;
            company.Address = dto.Address;
            company.Industry = dto.Industry;

            await _context.SaveChangesAsync();

            return MapToCompanyResponseDto(company);
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var workspaceId = _currentUserService.GetWorkspaceId();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.IdCompany == id && c.WorkspaceId == workspaceId);

            if (company == null)
            {
                return false;
            }

            var logoUrl = company.LogoUrl;

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrWhiteSpace(logoUrl))
            {
                await _cloudinaryService.DeleteImageAsync(logoUrl);
            }

            return true;
        }
    }
}