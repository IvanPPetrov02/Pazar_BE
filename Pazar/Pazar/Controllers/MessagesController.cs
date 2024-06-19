using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using BLL.DTOs;
using BLL.ManagerInterfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Pazar.Controllers
{
    [ApiController]
    [Route("ws/[controller]")]
    public class MessagesController : Controller
    {
        private readonly IChatManager _chatManager;
        private readonly IUserManager _userManager;
        private static ConcurrentDictionary<string, WebSocket> _webSockets = new ConcurrentDictionary<string, WebSocket>();

        public MessagesController(IChatManager chatManager, IUserManager userManager)
        {
            _chatManager = chatManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                string socketId = Guid.NewGuid().ToString();
                _webSockets.TryAdd(socketId, webSocket);

                await HandleWebSocketConnection(socketId, webSocket);

                _webSockets.TryRemove(socketId, out _);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task HandleWebSocketConnection(string socketId, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), HttpContext.RequestAborted);
                while (!result.CloseStatus.HasValue)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessage(socketId, message);
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), HttpContext.RequestAborted);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"WebSocket connection error: {ex.Message}");
            }
            finally
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", HttpContext.RequestAborted);
            }
        }

        private async Task ProcessMessage(string socketId, string message)
        {
            try
            {
                var messageData = JsonConvert.DeserializeObject<ChatMessageDTO>(message);
                if (messageData != null)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    if (userIdClaim == null)
                    {
                        throw new InvalidOperationException("User information is missing in the token.");
                    }

                    var userId = userIdClaim.Value;

                    await _chatManager.CreateChatOrMessageAsync(messageData.ItemSoldId, messageData.BuyerId, new MessageDTO
                    {
                        SenderId = userId,
                        SentAt = DateTime.UtcNow,
                        MessageSent = messageData.MessageSent
                    });

                    var messages = await _chatManager.GetMessagesByChatAsync(messageData.ChatId);
                    var messagesWithNames = await AddUserNamesToMessages(messages);
                    var jsonResponse = JsonConvert.SerializeObject(messagesWithNames);
                    var serverMsg = Encoding.UTF8.GetBytes(jsonResponse);

                    foreach (var ws in _webSockets.Values)
                    {
                        if (ws.State == WebSocketState.Open)
                        {
                            await ws.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), WebSocketMessageType.Text, true, HttpContext.RequestAborted);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing message: {ex.Message}");
            }
        }

        [HttpGet("messages/{chatId}")]
        public async Task<IActionResult> GetMessagesByChat(int chatId)
        {
            var messages = await _chatManager.GetMessagesByChatAsync(chatId);
            var messagesWithNames = await AddUserNamesToMessages(messages);
            return Ok(messagesWithNames);
        }

        private async Task<IEnumerable<MessageWithNameDTO>> AddUserNamesToMessages(IEnumerable<MessageDTO> messages)
        {
            var messagesWithNames = new List<MessageWithNameDTO>();

            foreach (var message in messages)
            {
                var sender = await _userManager.GetUserByIdAsync(message.SenderId);
                var senderName = sender != null ? $"{sender.Name} {sender.Surname}" : "Unknown User";

                messagesWithNames.Add(new MessageWithNameDTO
                {
                    Id = message.Id,
                    MessageSent = message.MessageSent,
                    SentAt = message.SentAt,
                    SenderName = senderName,
                    SenderId = message.SenderId
                });
            }

            return messagesWithNames;
        }
    }
}
