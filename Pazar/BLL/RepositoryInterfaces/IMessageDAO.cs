using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Chat_related;

namespace BLL.RepositoryInterfaces
{
    public interface IMessageDAO
    {
        Task CreateMessageAsync(Message message);
        Task DeleteMessageAsync(int id);
        Task UpdateMessageAsync(Message message);
        Task<IEnumerable<Message>> GetAllMessagesAsync();
        Task<Message> GetMessageByIdAsync(int id);
        Task<IEnumerable<Message>> GetMessagesByChatAsync(int chatId);
    }
}