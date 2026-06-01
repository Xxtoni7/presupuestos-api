using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.DTOs.Presupuesto;
using PresupuestosAPI.Services;
using PresupuestosAPI.Exceptions;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PresupuestoController : ControllerBase
    {
        private readonly PresupuestoService _presupuestoService;

        public PresupuestoController(PresupuestoService presupuestoService)
        {
            _presupuestoService = presupuestoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPresupuestos()
        {
            var presupuestos = await _presupuestoService.GetAllPresupuestosAsync();
            return Ok(presupuestos);
        }

        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetPresupuestosByCompanyId(int companyId)
        {
            var presupuestos = await _presupuestoService.GetPresupuestosByCompanyIdAsync(companyId);
            return Ok(presupuestos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPresupuestoById(int id)
        {
            var presupuesto = await _presupuestoService.GetPresupuestoByIdAsync(id);
            if (presupuesto == null)
            {
                return NotFound();
            }
            
            return Ok(presupuesto);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetPresupuestosByTitle([FromQuery] string title)
        {
            var presupuestos = await _presupuestoService.GetPresupuestosByTitleAsync(title);
            if (!presupuestos.Any())
            {
                return NotFound();
            }

            return Ok(presupuestos);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePresupuesto([FromBody] CreatePresupuestoDto dto)
        {
            try
            {
                var createdPresupuesto = await _presupuestoService.CreatePresupuestoAsync(dto);

                return CreatedAtAction(
                    nameof(GetPresupuestoById),
                    new { id = createdPresupuesto.IdPresupuesto },
                    createdPresupuesto
                );
            }
            catch (PlanLimitExceededException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound(new { message = "Empresa no encontrada." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePresupuesto(int id, [FromBody] UpdatePresupuestoDto dto)
        {
            var updatedPresupuesto = await _presupuestoService.UpdatePresupuestoAsync(id, dto);
            if (updatedPresupuesto == null)
            {
                return NotFound();
            }
            return Ok(updatedPresupuesto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePresupuesto(int id)
        {
            var deletedPresupuesto = await _presupuestoService.DeletePresupuestoAsync(id);
            if (!deletedPresupuesto) 
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}