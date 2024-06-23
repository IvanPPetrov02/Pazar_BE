using BLL.DTOs.ItemDTOs;
using BLL.Item_related;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.ManagerInterfaces;
using System.Security.Claims;
using BLL.Services;
using CustomAuthorization;
using Newtonsoft.Json;

namespace Pazar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemController : ControllerBase
    {
        private readonly IItemManager _itemManager;
        private readonly IJwtService _jwtService;

        public ItemController(IItemManager itemManager, IJwtService jwtService)
        {
            _itemManager = itemManager;
            _jwtService = jwtService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            try
            {
                Console.WriteLine($"Fetching item with ID: {id}");
                var item = await _itemManager.GetItemByIdAsync(id);
                if (item == null)
                {
                    Console.WriteLine($"Item with ID: {id} not found.");
                    return NotFound(new { Message = "Item not found." });
                }
                return Ok(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching item with ID: {id}, Error: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await _itemManager.GetAllItemsAsync();
            return Ok(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDTO itemDto)
        {
            // Get the user ID from the claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            itemDto.SellerId = userIdClaim.Value;

            try
            {
                // Log the incoming item data for debugging
                Console.WriteLine($"Received item data: {JsonConvert.SerializeObject(itemDto)}");

                await _itemManager.CreateItemAsync(itemDto);
                return CreatedAtAction(nameof(GetItemById), new { id = itemDto.SubCategoryId }, itemDto);
            }
            catch (Exception ex)
            {
                // Log the error message for debugging
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }
        
        
        [Authorize]
        [HttpPut("{id}")]
        [ServiceFilter(typeof(AdminOrOwnerAttribute))]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemUpdateDTO itemDto)
        {
            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }

            try
            {
                itemDto.Id = id;
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


        [Authorize]
        [HttpDelete("{id}")]
        [ServiceFilter(typeof(AdminOrOwnerAttribute))]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }
    
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

        [Authorize]
        [HttpPut("{id}/status")]
        [ServiceFilter(typeof(AdminOrOwnerAttribute))]
        public async Task<IActionResult> UpdateItemStatus(int id, [FromBody] ItemStatus status)
        {
            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }
    
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
        
        [Authorize]
        [HttpGet("{id}/isseller")]
        public async Task<IActionResult> IsUserSeller(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                var isSeller = await _itemManager.IsUserSellerAsync(id, userIdClaim.Value);
                return Ok(new { isSeller });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        
        
        [HttpGet("category/{parentCategoryId}")]
        public async Task<IActionResult> GetItemsByParentCategory(int parentCategoryId)
        {
            try
            {
                var items = await _itemManager.GetItemsByParentCategoryAsync(parentCategoryId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        
        [HttpGet("filtered")]
        public async Task<IActionResult> GetAllItemsFiltered()
        {
            var items = await _itemManager.GetAllItemsFilteredAsync();
            return Ok(items);
        }
        
        [HttpGet("subcategory/{subCategoryId}")]
        public async Task<IActionResult> GetItemsBySubCategory(int subCategoryId)
        {
            var items = await _itemManager.GetItemsBySubCategoryAsync(subCategoryId);
            return Ok(items);
        }
        
        [Authorize]
        [HttpGet("itemsbyseller/{sellerId}")]
        public async Task<IActionResult> GetItemsBySeller(string sellerId)
        {
            try
            {
                var items = await _itemManager.GetItemsBySellerAsync(sellerId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}
