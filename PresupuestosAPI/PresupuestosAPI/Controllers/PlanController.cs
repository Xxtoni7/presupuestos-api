using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.Services;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PlanController : ControllerBase
    {
        private readonly PlanLimitService _planLimitService;

        public PlanController(PlanLimitService planLimitService)
        {
            _planLimitService = planLimitService;
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentPlan()
        {
            try
            {
                var currentPlan = await _planLimitService.GetCurrentPlanUsageAsync();

                return Ok(currentPlan);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}