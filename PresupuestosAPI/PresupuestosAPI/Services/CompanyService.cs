using Microsoft.EntityFrameworkCore;
using PresupuestosAPI.Data;
using PresupuestosAPI.Models;

namespace PresupuestosAPI.Services
{
    public class CompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            return await _context.Companies.ToListAsync();
        }

        public async Task<Company?> GetCompanyByIdAsync(int id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task<List<Company>> GetCompaniesByNameAsync(string name)
        {
            return await _context.Companies
                .Where(c => c.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<Company> CreateCompanyAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<Company> UpdateCompanyAsync(int id, Company updatedCompany)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return null;
            }

            company.Name = updatedCompany.Name;
            company.LogoUrl = updatedCompany.LogoUrl;
            company.ColorMain = updatedCompany.ColorMain;
            company.ColorSecondary = updatedCompany.ColorSecondary;

            await _context.SaveChangesAsync();
            return company;
        }

        public async Task<bool> DeleteCompanyAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return false;
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
            return true;
        }


    }
}
