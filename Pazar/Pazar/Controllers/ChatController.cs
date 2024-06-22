using BLL.DTOs;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using CustomAuthorization;
using Microsoft.Extensions.Logging;

namespace Pazar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatManager _chatManager;
        private readonly ILogger<ChatController> _logger;
        private readonly IItemManager _itemManager;

        public ChatController(IChatManager chatManager, ILogger<ChatController> logger, IItemManager itemManager)
        {
            _chatManager = chatManager;
            _logger = logger;
            _itemManager = itemManager;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(IsSellerOrBuyerAttribute))]
        public async Task<IActionResult> GetChatById(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching chat with ID: {id}");
                var chat = await _chatManager.GetChatByIdAsync(id);
                if (chat == null)
                {
                    _logger.LogWarning($"Chat with ID: {id} not found.");
                    return NotFound(new { Message = "Chat not found." });
                }
                return Ok(chat);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching chat with ID: {id}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatsForUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                _logger.LogWarning("Invalid token.");
                return Unauthorized(new { message = "Invalid token." });
            }

            var userId = userIdClaim.Value;

            try
            {
                var chats = await _chatManager.GetChatsForUserAsync(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching chats for user with ID: {userId}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var buyerId = userIdClaim?.Value;

            if (string.IsNullOrEmpty(buyerId))
            {
                return Unauthorized(new { Message = "User information is missing in the token." });
            }
            
            if (dto.BuyerId != buyerId)
            {
                return BadRequest(new { Message = "Buyer ID in the token does not match the one in the request body." });
            }

            try
            {
                _logger.LogInformation($"Creating chat for ItemSoldId: {dto.ItemSoldId}, BuyerId: {buyerId}");
                
                var item = await _itemManager.GetItemByIdAsync(dto.ItemSoldId);
                if (item == null)
                {
                    return NotFound(new { Message = "Item not found." });
                }

                if (item.Seller.UUID.ToString() == buyerId)
                {
                    return new ObjectResult(new { Message = "User is the seller of the item." }) { StatusCode = 403 };
                }

                await _chatManager.CreateChatAsync(dto.ItemSoldId, buyerId, new MessageDTO
                {
                    SenderId = buyerId,
                    SentAt = DateTime.UtcNow,
                    MessageSent = dto.MessageSent
                });

                return Ok(new { Message = "Chat created successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating chat: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
