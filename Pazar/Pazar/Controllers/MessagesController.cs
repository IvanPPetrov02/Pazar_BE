using BLL.DTOs;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using CustomAuthorization;

namespace Pazar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageManager _messageManager;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IMessageManager messageManager, ILogger<MessageController> logger)
        {
            _messageManager = messageManager;
            _logger = logger;
        }

        [HttpGet("{chatId}")]
        [ServiceFilter(typeof(IsSellerOrBuyerAttribute))]
        public async Task<IActionResult> GetMessagesByChat(int chatId)
        {
            try
            {
                var messages = await _messageManager.GetMessagesByChatAsync(chatId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching messages for chat with ID: {chatId}");
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}