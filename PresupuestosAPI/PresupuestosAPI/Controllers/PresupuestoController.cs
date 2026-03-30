using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.Models;
using PresupuestosAPI.Services;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
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
            if (!presupuestos.Any())
            {
                return NotFound();
            }

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
        public async Task<IActionResult> CreatePresupuesto([FromBody] Presupuesto presupuesto)
        {
            var createdPresupuesto = await _presupuestoService.CreatePresupuestoAsync(presupuesto);
            return CreatedAtAction(
                nameof(GetPresupuestoById),
                new { id = createdPresupuesto.IdPresupuesto },
                createdPresupuesto
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePresupuesto(int id, [FromBody] Presupuesto presupuesto)
        {
            var updatedPresupuesto = await _presupuestoService.UpdatePresupuestoAsync(id, presupuesto);
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
