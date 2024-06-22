using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Chat_related;
using BLL.DTOs;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;

namespace BLL.Managers
{
    public class MessageManager : IMessageManager
    {
        private readonly IMessageDAO _messageDAO;
        private readonly IUserDAO _userDAO;
        private readonly IChatDAO _chatDAO;

        public MessageManager(IMessageDAO messageDAO, IUserDAO userDAO, IChatDAO chatDAO)
        {
            _messageDAO = messageDAO;
            _userDAO = userDAO;
            _chatDAO = chatDAO;
        }

        public async Task<IEnumerable<MessageDTO>> GetMessagesByChatAsync(int chatId)
        {
            var messages = await _messageDAO.GetMessagesByChatAsync(chatId);
            return messages.Select(m => new MessageDTO
            {
                Id = m.Id,
                SenderId = m.Sender?.UUID.ToString(),
                SentAt = m.SentAt,
                MessageSent = m.MessageSent,
                ChatId = m.Chat.Id
            });
        }

        public async Task UpdateMessageAsync(MessageDTO messageDto)
        {
            var message = await _messageDAO.GetMessageByIdAsync(messageDto.Id);
            var sender = await _userDAO.GetUserByIdAsync(messageDto.SenderId);

            if (message == null || sender == null)
            {
                throw new ArgumentException("Invalid message or sender information.");
            }

            message.Sender = sender;
            message.SentAt = messageDto.SentAt;
            message.MessageSent = messageDto.MessageSent;

            await _messageDAO.UpdateMessageAsync(message);
        }

        public async Task DeleteMessageAsync(int id)
        {
            await _messageDAO.DeleteMessageAsync(id);
        }

        public async Task CreateMessageAsync(MessageDTO messageDto)
        {
            var chat = await _chatDAO.GetChatByIdAsync(messageDto.ChatId);
            var sender = await _userDAO.GetUserByIdAsync(messageDto.SenderId);

            if (chat == null || sender == null)
            {
                throw new ArgumentException("Invalid chat or sender information.");
            }

            var message = new Message
            {
                Sender = sender,
                SentAt = messageDto.SentAt,
                MessageSent = messageDto.MessageSent,
                Chat = chat
            };

            await _messageDAO.CreateMessageAsync(message);
        }
    }
}
