using BLL.DTOs.ItemDTOs;
using BLL.Item_related;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.ManagerInterfaces;

namespace Pazar.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemManager _itemManager;

        public ItemController(IItemManager itemManager)
        {
            _itemManager = itemManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            try
            {
                var item = await _itemManager.GetItemByIdAsync(id);
                return Ok(item);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _itemManager.GetAllItemsAsync();
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDTO itemDto)
        {
            try
            {
                await _itemManager.CreateItemAsync(itemDto);
                return CreatedAtAction(nameof(GetItemById), new { id = itemDto.SubCategoryId }, itemDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemUpdateDTO itemDto)
        {
            try
            {
                itemDto.Id = id; // Ensure the ID is set correctly
                await _itemManager.UpdateItemAsync(itemDto);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                await _itemManager.DeleteItemAsync(id);
                return Ok(new { Message = "Item successfully deleted." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateItemStatus(int id, [FromBody] ItemStatus status)
        {
            try
            {
                await _itemManager.UpdateItemStatusAsync(id, status);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}/images")]
        public async Task<IActionResult> UpdateItemImages(int id, [FromBody] List<ItemImageDTO> images)
        {
            try
            {
                var imageEntities = images.Select(img => new ItemImages { Image = img.Image }).ToList();
                await _itemManager.UpdateItemImagesAsync(id, imageEntities);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
