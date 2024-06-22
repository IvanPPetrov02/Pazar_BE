using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Chat_related;
using BLL.RepositoryInterfaces;

namespace UnitTests.FakeDAL
{
    public class MessageDAOFake : IMessageDAO
    {
        private readonly List<Message> _messages = new List<Message>();

        public Task<IEnumerable<Message>> GetMessagesByChatAsync(int chatId)
        {
            var messages = _messages.Where(m => m.Chat.Id == chatId);
            return Task.FromResult(messages.AsEnumerable());
        }

        public Task<Message> GetMessageByIdAsync(int id)
        {
            var message = _messages.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(message);
        }

        public Task CreateMessageAsync(Message message)
        {
            message.Id = _messages.Count + 1;
            _messages.Add(message);
            return Task.CompletedTask;
        }

        public Task UpdateMessageAsync(Message message)
        {
            var existingMessage = _messages.FirstOrDefault(m => m.Id == message.Id);
            if (existingMessage != null)
            {
                _messages.Remove(existingMessage);
                _messages.Add(message);
            }
            return Task.CompletedTask;
        }

        public Task DeleteMessageAsync(int id)
        {
            var message = _messages.FirstOrDefault(m => m.Id == id);
            if (message != null)
            {
                _messages.Remove(message);
            }
            return Task.CompletedTask;
        }
    }
}