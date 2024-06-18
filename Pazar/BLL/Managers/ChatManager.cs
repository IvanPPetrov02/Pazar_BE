using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Chat_related;
using BLL.DTOs;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;

namespace BLL.Managers
{
    public class ChatManager : IChatManager
    {
        private readonly IChatDAO _chatDAO;
        private readonly IMessageDAO _messageDAO;
        private readonly IUserDAO _userDAO;
        private readonly IItemDAO _itemDAO;

        public ChatManager(IChatDAO chatDAO, IMessageDAO messageDAO, IUserDAO userDAO, IItemDAO itemDAO)
        {
            _chatDAO = chatDAO;
            _messageDAO = messageDAO;
            _userDAO = userDAO;
            _itemDAO = itemDAO;
        }

        public async Task CreateChatOrMessageAsync(int itemSoldId, string buyerId, MessageDTO messageDto)
        {
            var chats = await _chatDAO.GetAllChatsAsync();
            var existingChat = chats.FirstOrDefault(c => c.ItemSold.Id == itemSoldId && c.Buyer.UUID.ToString() == buyerId);
            var sender = await _userDAO.GetUserByIdAsync(messageDto.SenderId);

            if (sender == null)
            {
                throw new ArgumentException("Invalid sender information.");
            }

            if (existingChat == null)
            {
                var item = await _itemDAO.GetItemByIdAsync(itemSoldId);
                var buyer = await _userDAO.GetUserByIdAsync(buyerId);

                if (item == null || buyer == null)
                {
                    throw new ArgumentException("Invalid item or buyer information.");
                }

                var chat = new Chat
                {
                    ItemSold = item,
                    Buyer = buyer
                };

                await _chatDAO.CreateChatAsync(chat);

                var message = new Message
                {
                    Sender = sender,
                    SentAt = messageDto.SentAt,
                    MessageSent = messageDto.MessageSent,
                    Chat = chat
                };

                await _messageDAO.CreateMessageAsync(message);
            }
            else
            {
                var message = new Message
                {
                    Sender = sender,
                    SentAt = messageDto.SentAt,
                    MessageSent = messageDto.MessageSent,
                    Chat = existingChat
                };

                await _messageDAO.CreateMessageAsync(message);
            }
        }

        public async Task<ChatDTO> GetChatByIdAsync(int id)
        {
            var chat = await _chatDAO.GetChatByIdAsync(id);

            if (chat == null)
            {
                return null;
            }

            return new ChatDTO
            {
                Id = chat.Id,
                ItemSoldId = chat.ItemSold?.Id ?? 0,
                BuyerId = chat.Buyer?.UUID.ToString(),
                SellerId = chat.ItemSold?.Seller?.UUID.ToString()
            };
        }

        public async Task<IEnumerable<ChatDTO>> GetChatsForUserAsync(string userId)
        {
            var chats = await _chatDAO.GetAllChatsAsync();
            return chats
                .Where(chat => chat.Buyer?.UUID.ToString() == userId || chat.ItemSold?.Seller?.UUID.ToString() == userId)
                .Select(chat => new ChatDTO
                {
                    Id = chat.Id,
                    ItemSoldId = chat.ItemSold?.Id ?? 0,
                    BuyerId = chat.Buyer?.UUID.ToString(),
                    SellerId = chat.ItemSold?.Seller?.UUID.ToString()
                })
                .Where(chatDto => chatDto.BuyerId != null && chatDto.SellerId != null)
                .ToList();
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
    }
}
