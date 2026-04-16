using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.DTOs.Upload;
using PresupuestosAPI.Services;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly UploadService _uploadService;

        public UploadController(UploadService uploadService)
        {
            _uploadService = uploadService;
        }

        [HttpPost("logo")]
        public async Task<ActionResult<UploadLogoResponseDto>> UploadLogo(IFormFile file, [FromForm] string? oldLogoUrl)
        {
            try
            {
                var result = await _uploadService.UploadLogoAsync(file, oldLogoUrl);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}