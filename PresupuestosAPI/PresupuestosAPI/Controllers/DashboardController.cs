using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.Services;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var summary = await _dashboardService.GetSummaryAsync();

                return Ok(summary);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuario no autenticado." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}