using BLL.DTOs.ItemDTOs;
using BLL.Item_related;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BLL.ManagerInterfaces;
using System.Security.Claims;
using BLL.Services;

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

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateItem([FromBody] ItemCreateDTO itemDto)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            itemDto.SellerId = userIdClaim.Value;

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

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] ItemUpdateDTO itemDto)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }

            if (!_jwtService.IsAdminOrOwner(principal, item.Seller.UUID.ToString()))
            {
                return Unauthorized(new { Message = "You are not authorized to update this item." });
            }

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

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }

            if (!_jwtService.IsAdminOrOwner(principal, item.Seller.UUID.ToString()))
            {
                return Unauthorized(new { Message = "You are not authorized to delete this item." });
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
        public async Task<IActionResult> UpdateItemStatus(int id, [FromBody] ItemStatus status)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }

            if (!_jwtService.IsAdminOrOwner(principal, item.Seller.UUID.ToString()))
            {
                return Unauthorized(new { Message = "You are not authorized to update the status of this item." });
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
        [HttpPut("{id}/images")]
        public async Task<IActionResult> UpdateItemImages(int id, [FromBody] List<ItemImageDTO> images)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

            var item = await _itemManager.GetItemByIdAsync(id);
            if (item == null)
            {
                return NotFound(new { Message = "Item not found" });
            }

            if (!_jwtService.IsAdminOrOwner(principal, item.Seller.UUID.ToString()))
            {
                return Unauthorized(new { Message = "You are not authorized to update the images of this item." });
            }

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
