using System.ComponentModel.DataAnnotations;

namespace PresupuestosAPI.DTOs.Company
{
    public class CreateCompanyDto
    {
        [Required]
        [MaxLength(60)]
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? ColorMain { get; set; }
        public string? ColorSecondary { get; set; }
        [MaxLength(80)]
        public string? Phone { get; set; }
        [MaxLength(80)]
        [EmailAddress]
        public string? Email { get; set; }
        [MaxLength(80)]
        public string? Address { get; set; }
        public string? Industry { get; set; }
    }
}