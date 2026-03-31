using Microsoft.AspNetCore.Mvc;
using PresupuestosAPI.Models;
using PresupuestosAPI.Services;

namespace PresupuestosAPI.Controllers
{
    [ApiController]
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
            if (!items.Any())
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
        public async Task<IActionResult> CreateItem([FromBody] PresupuestoItem item)
        {
            var createdItem = await _itemService.CreateItemAsync(item);
            return CreatedAtAction(
                nameof(GetItemById),
                new { id = createdItem.IdItem },
                createdItem
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] PresupuestoItem item)
        {
            var updatedItem = await _itemService.UpdateItemAsync(id, item);
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
