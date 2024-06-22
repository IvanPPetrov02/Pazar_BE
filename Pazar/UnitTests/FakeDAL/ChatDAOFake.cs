using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Chat_related;
using BLL.RepositoryInterfaces;

namespace UnitTests.FakeDAL
{
    public class ChatDAOFake : IChatDAO
    {
        private readonly List<Chat> _chats = new List<Chat>();

        public Task CreateChatAsync(Chat chat)
        {
            chat.Id = _chats.Count + 1;
            _chats.Add(chat);
            return Task.CompletedTask;
        }

        public Task<Chat> GetChatByIdAsync(int id)
        {
            var chat = _chats.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(chat);
        }

        public Task<IEnumerable<Chat>> GetAllChatsAsync()
        {
            return Task.FromResult(_chats.AsEnumerable());
        }

        public Task UpdateChatAsync(Chat chat)
        {
            var existingChat = _chats.FirstOrDefault(c => c.Id == chat.Id);
            if (existingChat != null)
            {
                _chats.Remove(existingChat);
                _chats.Add(chat);
            }
            return Task.CompletedTask;
        }

        public Task DeleteChatAsync(int id)
        {
            var chat = _chats.FirstOrDefault(c => c.Id == id);
            if (chat != null)
            {
                _chats.Remove(chat);
            }
            return Task.CompletedTask;
        }
    }
}