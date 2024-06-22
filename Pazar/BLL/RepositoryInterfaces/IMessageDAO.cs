using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Chat_related;

namespace BLL.RepositoryInterfaces
{
    public interface IMessageDAO
    {
        Task<IEnumerable<Message>> GetMessagesByChatAsync(int chatId);
        Task<Message> GetMessageByIdAsync(int id);
        Task CreateMessageAsync(Message message);
        Task UpdateMessageAsync(Message message);
        Task DeleteMessageAsync(int id);
    }
}