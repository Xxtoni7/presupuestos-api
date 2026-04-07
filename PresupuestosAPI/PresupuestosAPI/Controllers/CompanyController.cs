using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.Services;
using PresupuestosAPI.DTOs.Company;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly CompanyService _companyService;

        public CompanyController(CompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetCompanyByName([FromQuery] string Name)
        {
            var companies = await _companyService.GetCompaniesByNameAsync(Name);
            if (!companies.Any())
            {
                return NotFound();
            }

            return Ok(companies);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDto dto)
        {
            var createdCompany = await _companyService.CreateCompanyAsync(dto);
            return CreatedAtAction(
                nameof(GetCompanyById),
                new { Id = createdCompany.IdCompany },
                createdCompany
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyDto dto)
        {
            var updatedCompany = await _companyService.UpdateCompanyAsync(id, dto);
            if (updatedCompany == null)
            {
                return NotFound();
            }

            return Ok(updatedCompany);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var deletedCompany = await _companyService.DeleteCompanyAsync(id);
            if (!deletedCompany) 
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
