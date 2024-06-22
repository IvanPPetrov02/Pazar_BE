using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using BLL.Chat_related;
using BLL.DTOs;
using BLL.Managers;
using NUnit.Framework;
using UnitTests.FakeDAL;

namespace UnitTests
{
    [TestFixture]
    public class MessageManagerTests
    {
        private MessageManager _messageManager;
        private MessageDAOFake _messageDaoFake;
        private UserDAOFake _userDaoFake;
        private ChatDAOFake _chatDaoFake;

        [SetUp]
        public void Setup()
        {
            _messageDaoFake = new MessageDAOFake();
            _userDaoFake = new UserDAOFake();
            _chatDaoFake = new ChatDAOFake();
            _messageManager = new MessageManager(_messageDaoFake, _userDaoFake, _chatDaoFake);
        }

        [Test]
        public async Task GetMessagesByChatAsync_ShouldReturnMessages_WhenMessagesExist()
        {
            var chatId = 1;
            var chat = new Chat { Id = chatId };
            await _chatDaoFake.CreateChatAsync(chat);

            var message = new Message
            {
                Chat = chat,
                Sender = new User { UUID = Guid.NewGuid() },
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };
            await _messageDaoFake.CreateMessageAsync(message);

            var result = await _messageManager.GetMessagesByChatAsync(chatId);

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Hello", result.First().MessageSent);
        }

        [Test]
        public async Task GetMessagesByChatAsync_ShouldReturnEmptyList_WhenNoMessagesExist()
        {
            var chatId = 1;
            var chat = new Chat { Id = chatId };
            await _chatDaoFake.CreateChatAsync(chat);

            var result = await _messageManager.GetMessagesByChatAsync(chatId);

            Assert.NotNull(result);
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task UpdateMessageAsync_ShouldUpdateMessage_WhenMessageExists()
        {
            var user = new User { UUID = Guid.NewGuid() };
            await _userDaoFake.CreateUserAsync(user);
            var message = new Message
            {
                Id = 1,
                Sender = user,
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };
            await _messageDaoFake.CreateMessageAsync(message);

            var messageDto = new MessageDTO
            {
                Id = 1,
                SenderId = user.UUID.ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Updated Hello"
            };

            await _messageManager.UpdateMessageAsync(messageDto);

            var updatedMessage = await _messageDaoFake.GetMessageByIdAsync(1);
            Assert.AreEqual("Updated Hello", updatedMessage.MessageSent);
        }

        [Test]
        public void UpdateMessageAsync_ShouldThrowException_WhenMessageDoesNotExist()
        {
            var messageDto = new MessageDTO
            {
                Id = 999,
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };

            Assert.ThrowsAsync<ArgumentException>(() => _messageManager.UpdateMessageAsync(messageDto));
        }

        [Test]
        public void UpdateMessageAsync_ShouldThrowException_WhenSenderDoesNotExist()
        {
            var message = new Message
            {
                Id = 1,
                Sender = new User { UUID = Guid.NewGuid() },
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };
            _messageDaoFake.CreateMessageAsync(message);

            var messageDto = new MessageDTO
            {
                Id = 1,
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Updated Hello"
            };

            Assert.ThrowsAsync<ArgumentException>(() => _messageManager.UpdateMessageAsync(messageDto));
        }

        [Test]
        public async Task DeleteMessageAsync_ShouldDeleteMessage_WhenMessageExists()
        {
            var message = new Message
            {
                Id = 1,
                Sender = new User { UUID = Guid.NewGuid() },
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };
            await _messageDaoFake.CreateMessageAsync(message);

            await _messageManager.DeleteMessageAsync(1);

            var deletedMessage = await _messageDaoFake.GetMessageByIdAsync(1);
            Assert.IsNull(deletedMessage);
        }

        [Test]
        public async Task CreateMessageAsync_ShouldCreateNewMessage_WhenValidChatAndSenderExist()
        {
            var chatId = 1;
            var senderId = Guid.NewGuid().ToString();
            var chat = new Chat { Id = chatId };
            var sender = new User { UUID = Guid.Parse(senderId) };

            await _chatDaoFake.CreateChatAsync(chat);
            await _userDaoFake.CreateUserAsync(sender);

            var messageDto = new MessageDTO
            {
                SenderId = senderId,
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello",
                ChatId = chatId
            };

            await _messageManager.CreateMessageAsync(messageDto);

            var messages = await _messageDaoFake.GetMessagesByChatAsync(chatId);
            Assert.AreEqual(1, messages.Count());
            Assert.AreEqual("Hello", messages.First().MessageSent);
        }

        [Test]
        public void CreateMessageAsync_ShouldThrowException_WhenChatDoesNotExist()
        {
            var chatId = 999;
            var senderId = Guid.NewGuid().ToString();

            var sender = new User { UUID = Guid.Parse(senderId) };
            _userDaoFake.CreateUserAsync(sender);

            var messageDto = new MessageDTO
            {
                SenderId = senderId,
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello",
                ChatId = chatId
            };

            Assert.ThrowsAsync<ArgumentException>(() => _messageManager.CreateMessageAsync(messageDto));
        }

        [Test]
        public void CreateMessageAsync_ShouldThrowException_WhenSenderDoesNotExist()
        {
            var chatId = 1;
            var senderId = Guid.NewGuid().ToString();
            var chat = new Chat { Id = chatId };
            _chatDaoFake.CreateChatAsync(chat);

            var messageDto = new MessageDTO
            {
                SenderId = senderId,
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello",
                ChatId = chatId
            };

            Assert.ThrowsAsync<ArgumentException>(() => _messageManager.CreateMessageAsync(messageDto));
        }
    }
}
