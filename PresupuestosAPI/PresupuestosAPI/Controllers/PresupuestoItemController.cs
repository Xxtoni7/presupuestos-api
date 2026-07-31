using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.DTOs.PresupuestoItem;
using PresupuestosAPI.Services;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PresupuestoItemController : ControllerBase
    {
        private readonly PresupuestoItemService _itemService;
        public PresupuestoItemController(PresupuestoItemService itemService)
        {
            _itemService = itemService;
        }

        [HttpGet("presupuesto/{presupuestoId}")]
        public async Task<IActionResult> GetItemByPresupuestoId(int presupuestoId)
        {
            var items = await _itemService.GetItemsByPresupuestoIdAsync(presupuestoId);
            if (items == null)
            {
                return NotFound();
            }
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            var item = await _itemService.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] CreatePresupuestoItemDto dto)
        {
            try
            {
                var createdItem = await _itemService.CreateItemAsync(dto);

                return CreatedAtAction(
                    nameof(GetItemById),
                    new { id = createdItem.IdItem },
                    createdItem
                );
            }
            catch (UnauthorizedAccessException)
            {
                return NotFound(new { message = "Presupuesto no encontrado." });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] UpdatePresupuestoItemDto dto)
        {
            var updatedItem = await _itemService.UpdateItemAsync(id, dto);
            if (updatedItem == null)
            {
                return NotFound();
            }
            return Ok(updatedItem);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var deleted = await _itemService.DeleteItemAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();

        }
    }
}
