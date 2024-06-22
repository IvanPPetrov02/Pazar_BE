using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.ManagerInterfaces
{
    public interface IMessageManager
    {
        Task<IEnumerable<MessageDTO>> GetMessagesByChatAsync(int chatId);
        Task UpdateMessageAsync(MessageDTO message);
        Task DeleteMessageAsync(int id);
        Task CreateMessageAsync(MessageDTO messageDto);
    }
}