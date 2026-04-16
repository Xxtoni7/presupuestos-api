using PresupuestosAPI.DTOs.Upload;

namespace PresupuestosAPI.Services
{
    public class UploadService
    {
        private readonly IWebHostEnvironment _environment;

        public UploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<UploadLogoResponseDto> UploadLogoAsync(IFormFile file, string? oldLogoUrl = null)
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("Debe seleccionar un archivo.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new Exception("Formato no permitido. Solo se aceptan .jpg, .jpeg, .png y .webp");
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var uploadsFolder = Path.Combine(
                webRootPath,
                "uploads",
                "companies",
                "logos"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            if (!string.IsNullOrWhiteSpace(oldLogoUrl))
            {
                DeleteOldLogoIfExists(oldLogoUrl, uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/companies/logos/{uniqueFileName}";

            return new UploadLogoResponseDto
            {
                FileName = uniqueFileName,
                Url = fileUrl
            };
        }

        private static void DeleteOldLogoIfExists(string oldLogoUrl, string uploadsFolder)
        {
            try
            {
                var oldFileName = Path.GetFileName(oldLogoUrl);

                if (string.IsNullOrWhiteSpace(oldFileName))
                {
                    return;
                }

                var oldFilePath = Path.Combine(uploadsFolder, oldFileName);

                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }
            catch
            {
                // Por ahora no rompemos el flujo si falla el borrado del logo viejo
            }
        }
    }
}