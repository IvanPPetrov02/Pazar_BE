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

        public async Task CreateChatAsync(int itemSoldId, string buyerId, MessageDTO messageDto)
        {
            var item = await _itemDAO.GetItemByIdAsync(itemSoldId);
            var buyer = await _userDAO.GetUserByIdAsync(buyerId);
            var sender = await _userDAO.GetUserByIdAsync(messageDto.SenderId);

            if (item == null || buyer == null || sender == null)
            {
                throw new ArgumentException("Invalid item, buyer, or sender information.");
            }
            
            var existingChats = await _chatDAO.GetAllChatsAsync();
            var existingChat = existingChats.FirstOrDefault(c => c.ItemSold.Id == itemSoldId && c.Buyer.UUID.ToString() == buyerId);

            if (existingChat == null)
            {
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
    }
}
