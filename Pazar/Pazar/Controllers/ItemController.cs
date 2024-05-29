using BLL.DTOs;
using BLL.Item_related;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs.ItemDTOs;

namespace Pazar.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemController : ControllerBase
    {
        private readonly IItemManager _itemManager;

        public ItemController(IItemManager itemManager)
        {
            _itemManager = itemManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDTO itemDto)
        {
            await _itemManager.CreateItemAsync(itemDto);
            return Ok(new { message = "Item created" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemUpdateDTO itemDto)
        {
            if (id != itemDto.Id)
            {
                return BadRequest(new { message = "Item ID mismatch" });
            }

            await _itemManager.UpdateItemAsync(itemDto);
            return Ok(new { message = "Item updated" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            await _itemManager.DeleteItemAsync(id);
            return Ok(new { message = "Item deleted" });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            var item = await _itemManager.GetItemByIdAsync(id);
            return item != null ? Ok(item) : NotFound(new { message = "Item not found" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _itemManager.GetAllItemsAsync();
            return Ok(items);
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateItemStatus(int id, [FromBody] ItemStatus status)
        {
            await _itemManager.UpdateItemStatusAsync(id, status);
            return Ok(new { message = "Item status updated" });
        }

        [HttpPut("images/{id}")]
        public async Task<IActionResult> UpdateItemImages(int id, [FromBody] List<ItemImages> images)
        {
            await _itemManager.UpdateItemImagesAsync(id, images);
            return Ok(new { message = "Item images updated" });
        }
    }
}