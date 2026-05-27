using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Company
{
    public class UpdateCompanyDto
    {
        [Required]
        [MaxLength(120)]
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? ColorMain { get; set; }
        public string? ColorSecondary { get; set; }
        [MaxLength(30)]
        public string? Phone { get; set; }
        [MaxLength(150)]
        [EmailAddress]
        public string? Email { get; set; }
        [MaxLength(200)]
        public string? Address { get; set; }
        public string? Industry { get; set; }
    }
}