using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.ManagerInterfaces
{
    public interface IChatManager
    {
        Task CreateChatAsync(int itemSoldId, string buyerId, MessageDTO message);
        Task<ChatDTO> GetChatByIdAsync(int id);
        Task<IEnumerable<ChatDTO>> GetChatsForUserAsync(string userId);
    }
}