using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using BLL.DTOs;
using BLL.ManagerInterfaces;
using Microsoft.Extensions.Logging;

namespace Pazar.Hubs
{
    public class MessageHub : Hub
    {
        private readonly IMessageManager _messageManager;
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(IMessageManager messageManager, ILogger<MessageHub> logger)
        {
            _messageManager = messageManager;
            _logger = logger;
        }

        public async Task SendMessage(MessageDTO messageDto)
        {
            try
            {
                // Save the message
                await _messageManager.CreateMessageAsync(messageDto);

                // Broadcast the message to all clients in the same chat room
                await Clients.Group(messageDto.ChatId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                throw;
            }
        }

        public async Task JoinChat(int chatId)
        {
            try
            {
                _logger.LogInformation($"Attempting to join chat {chatId} for connection {Context.ConnectionId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
                _logger.LogInformation($"Connection {Context.ConnectionId} joined chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining chat {chatId}: {ex.Message}");
                throw;
            }
        }


        public async Task LeaveChat(int chatId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
                _logger.LogInformation($"Connection {Context.ConnectionId} left chat {chatId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving chat {chatId}");
                throw;
            }
        }
    }
}
