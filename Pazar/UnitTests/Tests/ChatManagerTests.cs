using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using BLL.Chat_related;
using BLL.DTOs;
using BLL.Item_related;
using BLL.Managers;
using NUnit.Framework;
using UnitTests.FakeDAL;

namespace UnitTests
{
    [TestFixture]
    public class ChatManagerTests
    {
        private ChatManager _chatManager;
        private ChatDAOFake _chatDaoFake;
        private MessageDAOFake _messageDaoFake;
        private UserDAOFake _userDaoFake;
        private ItemDAOFake _itemDaoFake;

        [SetUp]
        public void Setup()
        {
            _chatDaoFake = new ChatDAOFake();
            _messageDaoFake = new MessageDAOFake();
            _userDaoFake = new UserDAOFake();
            _itemDaoFake = new ItemDAOFake();
            _chatManager = new ChatManager(_chatDaoFake, _messageDaoFake, _userDaoFake, _itemDaoFake);
        }

        [Test]
        public async Task CreateChatAsync_ShouldCreateNewChat_WhenChatDoesNotExist()
        {
            var itemSoldId = 1;
            var buyerId = Guid.NewGuid().ToString();
            var messageDto = new MessageDTO
            {
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };

            var item = new Item { Id = itemSoldId, Seller = new User() };
            await _itemDaoFake.CreateItemAsync(item);
            var buyer = new User { UUID = Guid.Parse(buyerId) };
            await _userDaoFake.CreateUserAsync(buyer);
            var sender = new User { UUID = Guid.Parse(messageDto.SenderId) };
            await _userDaoFake.CreateUserAsync(sender);

            await _chatManager.CreateChatAsync(itemSoldId, buyerId, messageDto);

            var chats = await _chatDaoFake.GetAllChatsAsync();
            Assert.AreEqual(1, chats.Count());
        }

        [Test]
        public async Task CreateChatAsync_ShouldAddMessageToExistingChat_WhenChatExists()
        {
            var itemSoldId = 1;
            var buyerId = Guid.NewGuid().ToString();
            var messageDto = new MessageDTO
            {
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };

            var item = new Item { Id = itemSoldId, Seller = new User() };
            await _itemDaoFake.CreateItemAsync(item);
            var buyer = new User { UUID = Guid.Parse(buyerId) };
            await _userDaoFake.CreateUserAsync(buyer);
            var sender = new User { UUID = Guid.Parse(messageDto.SenderId) };
            await _userDaoFake.CreateUserAsync(sender);

            var existingChat = new Chat { ItemSold = item, Buyer = buyer };
            await _chatDaoFake.CreateChatAsync(existingChat);

            await _chatManager.CreateChatAsync(itemSoldId, buyerId, messageDto);

            var messages = await _messageDaoFake.GetMessagesByChatAsync(existingChat.Id);
            Assert.AreEqual(1, messages.Count());
            Assert.AreEqual(existingChat.Id, messages.First().Chat.Id);
        }

        [Test]
        public async Task GetChatByIdAsync_ShouldReturnChat_WhenChatExists()
        {
            var chatId = 1;
            var chat = new Chat
            {
                Id = chatId,
                ItemSold = new Item { Id = 1, Seller = new User { UUID = Guid.NewGuid() } },
                Buyer = new User { UUID = Guid.NewGuid() }
            };

            await _chatDaoFake.CreateChatAsync(chat);

            var result = await _chatManager.GetChatByIdAsync(chatId);

            Assert.NotNull(result);
            Assert.AreEqual(chatId, result.Id);
        }

        [Test]
        public async Task GetChatByIdAsync_ShouldReturnNull_WhenChatDoesNotExist()
        {
            var chatId = 1;

            var result = await _chatManager.GetChatByIdAsync(chatId);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetChatsForUserAsync_ShouldReturnChats_WhenChatsExistForUser()
        {
            var userId = Guid.NewGuid().ToString();
            var user = new User { UUID = Guid.Parse(userId) };
            await _userDaoFake.CreateUserAsync(user);

            var chats = new List<Chat>
            {
                new Chat
                {
                    Id = 1,
                    ItemSold = new Item { Id = 1, Seller = user },
                    Buyer = new User { UUID = Guid.NewGuid() }
                },
                new Chat
                {
                    Id = 2,
                    ItemSold = new Item { Id = 2, Seller = new User { UUID = Guid.NewGuid() } },
                    Buyer = user
                }
            };

            foreach (var chat in chats)
            {
                await _chatDaoFake.CreateChatAsync(chat);
            }

            var result = await _chatManager.GetChatsForUserAsync(userId);

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task GetChatsForUserAsync_ShouldReturnEmptyList_WhenNoChatsExistForUser()
        {
            var userId = Guid.NewGuid().ToString();

            var result = await _chatManager.GetChatsForUserAsync(userId);

            Assert.NotNull(result);
            Assert.IsEmpty(result);
        }

        // Unhappy flow tests

        [Test]
        public void CreateChatAsync_ShouldThrowException_WhenItemDoesNotExist()
        {
            var itemSoldId = 999;
            var buyerId = Guid.NewGuid().ToString();
            var messageDto = new MessageDTO
            {
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };

            var buyer = new User { UUID = Guid.Parse(buyerId) };
            var sender = new User { UUID = Guid.Parse(messageDto.SenderId) };
            _userDaoFake.CreateUserAsync(buyer);
            _userDaoFake.CreateUserAsync(sender);

            Assert.ThrowsAsync<ArgumentException>(() => _chatManager.CreateChatAsync(itemSoldId, buyerId, messageDto));
        }

        [Test]
        public void CreateChatAsync_ShouldThrowException_WhenBuyerDoesNotExist()
        {
            var itemSoldId = 1;
            var buyerId = Guid.NewGuid().ToString();
            var messageDto = new MessageDTO
            {
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };

            var item = new Item { Id = itemSoldId, Seller = new User() };
            var sender = new User { UUID = Guid.Parse(messageDto.SenderId) };
            _itemDaoFake.CreateItemAsync(item);
            _userDaoFake.CreateUserAsync(sender);

            Assert.ThrowsAsync<ArgumentException>(() => _chatManager.CreateChatAsync(itemSoldId, buyerId, messageDto));
        }

        [Test]
        public void CreateChatAsync_ShouldThrowException_WhenSenderDoesNotExist()
        {
            var itemSoldId = 1;
            var buyerId = Guid.NewGuid().ToString();
            var messageDto = new MessageDTO
            {
                SenderId = Guid.NewGuid().ToString(),
                SentAt = DateTime.UtcNow,
                MessageSent = "Hello"
            };

            var item = new Item { Id = itemSoldId, Seller = new User() };
            var buyer = new User { UUID = Guid.Parse(buyerId) };
            _itemDaoFake.CreateItemAsync(item);
            _userDaoFake.CreateUserAsync(buyer);

            Assert.ThrowsAsync<ArgumentException>(() => _chatManager.CreateChatAsync(itemSoldId, buyerId, messageDto));
        }
    }
}
