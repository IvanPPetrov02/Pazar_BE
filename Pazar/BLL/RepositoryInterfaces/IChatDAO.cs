using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Chat_related;

namespace BLL.RepositoryInterfaces
{
    public interface IChatDAO
    {
        Task CreateChatAsync(Chat chat);
        Task<Chat> GetChatByIdAsync(int id);
        Task<IEnumerable<Chat>> GetAllChatsAsync();
        Task UpdateChatAsync(Chat chat);
        Task DeleteChatAsync(int id);
    }
}