using BLL.DTOs;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using BLL.Services;
using CustomAuthorization;

namespace Pazar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatManager _chatManager;
        private readonly IJwtService _jwtService;

        public ChatController(IChatManager chatManager, IJwtService jwtService)
        {
            _chatManager = chatManager;
            _jwtService = jwtService;
        }

        [HttpGet("{id}")]
        [ServiceFilter(typeof(IsSellerOrBuyerAttribute))]
        public async Task<IActionResult> GetChatById(int id)
        {
            try
            {
                Console.WriteLine($"Fetching chat with ID: {id}");
                var chat = await _chatManager.GetChatByIdAsync(id);
                if (chat == null)
                {
                    Console.WriteLine($"Chat with ID: {id} not found.");
                    return NotFound(new { Message = "Chat not found." });
                }
                return Ok(chat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching chat with ID: {id}, Error: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChatsForUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
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
                Console.WriteLine($"Error fetching chats for user with ID: {userId}, Error: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost]
        [ServiceFilter(typeof(IsNotSellerAttribute))]
        public async Task<IActionResult> CreateChatOrMessage([FromBody] CreateChatOrMessageDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            try
            {
                await _chatManager.CreateChatOrMessageAsync(dto.ItemSoldId, dto.BuyerId, new MessageDTO
                {
                    SenderId = userIdClaim.Value,
                    SentAt = DateTime.UtcNow,
                    MessageSent = dto.MessageSent
                });
                return Ok(new { Message = "Chat or message created successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{chatId}/messages")]
        [ServiceFilter(typeof(IsSellerOrBuyerAttribute))]
        public async Task<IActionResult> GetMessagesByChat(int chatId)
        {
            try
            {
                var messages = await _chatManager.GetMessagesByChatAsync(chatId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching messages for chat with ID: {chatId}, Error: {ex.Message}");
                return BadRequest(new { Message = ex.Message });
            }
        }

        /*
        [HttpPut("messages/{id}")]
        public async Task<IActionResult> UpdateMessage(int id, [FromBody] MessageDTO messageDto)
        {
            try
            {
                messageDto.Id = id;
                await _chatManager.UpdateMessageAsync(messageDto);
                return Ok(new { Message = "Message updated successfully." });
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

        [HttpDelete("messages/{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            try
            {
                await _chatManager.DeleteMessageAsync(id);
                return Ok(new { Message = "Message deleted successfully." });
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
        */
    }
}
