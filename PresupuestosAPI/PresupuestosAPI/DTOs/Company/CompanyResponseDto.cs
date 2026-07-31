namespace PresupuestosAPI.DTOs.Company
{
    public class CompanyResponseDto
    {
        public int IdCompany { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? ColorMain { get; set; }
        public string? ColorSecondary { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Industry { get; set; }
    }
}