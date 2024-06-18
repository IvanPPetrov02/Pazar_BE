using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.ManagerInterfaces
{
    public interface IChatManager
    {
        Task CreateChatOrMessageAsync(int itemSoldId, string buyerId, MessageDTO message);
        Task<ChatDTO> GetChatByIdAsync(int id);
        Task<IEnumerable<ChatDTO>> GetChatsForUserAsync(string userId);
        Task<IEnumerable<MessageDTO>> GetMessagesByChatAsync(int chatId);
        Task UpdateMessageAsync(MessageDTO message);
        Task DeleteMessageAsync(int id);
    }
}