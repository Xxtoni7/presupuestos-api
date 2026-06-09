namespace PresupuestosAPI.DTOs.Dashboard
{
    public class DashboardAlertDto
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? ActionLabel { get; set; }
        public string? ActionUrl { get; set; }
    }
}