using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.DTOs.Company;
using PresupuestosAPI.Models;
using Microsoft.AspNetCore.Hosting;

namespace PresupuestosAPI.Services
{
    public class CompanyService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CompanyService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
                Industry = company.Industry,
                IdUser = company.IdUser
            };
        }

        public async Task<List<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            var companies = await _context.Companies.ToListAsync();
            return companies.Select(MapToCompanyResponseDto).ToList();
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int id)
        {
            var company =  await _context.Companies.FindAsync(id);
            if(company == null) return null;

            return MapToCompanyResponseDto(company);
        }

        public async Task<List<CompanyResponseDto>> GetCompaniesByNameAsync(string name)
        {
            var companies = await _context.Companies
                .Where(c => c.Name.Contains(name))
                .ToListAsync();

            return companies.Select(MapToCompanyResponseDto).ToList();
        }

        public async Task<CompanyResponseDto> CreateCompanyAsync(CreateCompanyDto dto)
        {
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
                IdUser = dto.IdUser
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return MapToCompanyResponseDto(company);
        }

        public async Task<CompanyResponseDto?> UpdateCompanyAsync(int id, UpdateCompanyDto dto)
        {
            var company = await _context.Companies.FindAsync(id);
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
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(company.LogoUrl))
            {
                var webRootPath = _environment.WebRootPath;

                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
                }

                var relativePath = company.LogoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var logoPath = Path.Combine(webRootPath, relativePath);

                if (File.Exists(logoPath))
                {
                    File.Delete(logoPath);
                }
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
