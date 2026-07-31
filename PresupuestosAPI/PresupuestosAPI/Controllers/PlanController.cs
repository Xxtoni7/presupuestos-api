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
        private readonly PlanService _planService;

        public PlanController(PlanLimitService planLimitService, PlanService planService)
        {
            _planLimitService = planLimitService;
            _planService = planService;
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

        [HttpGet("available")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailablePlans()
        {
            var plans = await _planService.GetAvailablePlansAsync();

            return Ok(plans);
        }
    }
}