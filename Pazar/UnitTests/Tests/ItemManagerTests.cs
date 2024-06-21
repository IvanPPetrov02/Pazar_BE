using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL;
using BLL.Category_related;
using BLL.DTOs.ItemDTOs;
using BLL.ItemRelated;
using BLL.Item_related;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;
using Moq;
using NUnit.Framework;
using UnitTests.FakeDAL;

namespace UnitTests
{
    [TestFixture]
    public class ItemManagerTests
    {
        private ItemManager _itemManager;
        private ItemDAOFake _itemDaoFake;
        private Mock<ICategoryDAO> _categoryDaoMock;
        private Mock<IUserDAO> _userDaoMock;

        [SetUp]
        public void Setup()
        {
            _itemDaoFake = new ItemDAOFake();
            _categoryDaoMock = new Mock<ICategoryDAO>();
            _userDaoMock = new Mock<IUserDAO>();
            _itemManager = new ItemManager(_itemDaoFake, _categoryDaoMock.Object, _userDaoMock.Object);
        }

        [Test]
        public async Task CreateItemAsync_ValidItem_CreatesItem()
        {
            var itemDto = new ItemCreateDTO
            {
                Name = "New Item",
                Description = "Description",
                Price = 100,
                Images = null,
                SubCategoryId = 1,
                Condition = Condition.New,
                BidOnly = false,
                SellerId = "seller1"
            };

            var category = new Category { Id = 1, Name = "Category" };
            var seller = new User { UUID = Guid.NewGuid() };

            _categoryDaoMock.Setup(m => m.GetCategoryByIdAsync(itemDto.SubCategoryId)).ReturnsAsync(category);
            _userDaoMock.Setup(m => m.GetUserByIdAsync(itemDto.SellerId)).ReturnsAsync(seller);

            await _itemManager.CreateItemAsync(itemDto);

            var allItems = await _itemDaoFake.GetAllItemsAsync();
            Assert.AreEqual(1, allItems.Count());
        }

        [Test]
        public void CreateItemAsync_InvalidCategory_ThrowsException()
        {
            var itemDto = new ItemCreateDTO
            {
                Name = "New Item",
                Description = "Description",
                Price = 100,
                Images = null,
                SubCategoryId = 1,
                Condition = Condition.New,
                BidOnly = false,
                SellerId = "seller1"
            };

            _categoryDaoMock.Setup(m => m.GetCategoryByIdAsync(itemDto.SubCategoryId)).ThrowsAsync(new InvalidOperationException("Category not found."));
            _userDaoMock.Setup(m => m.GetUserByIdAsync(itemDto.SellerId)).ReturnsAsync(new User { UUID = Guid.NewGuid() });

            Assert.ThrowsAsync<InvalidOperationException>(() => _itemManager.CreateItemAsync(itemDto));
        }

        [Test]
        public void CreateItemAsync_InvalidSeller_ThrowsException()
        {
            var itemDto = new ItemCreateDTO
            {
                Name = "New Item",
                Description = "Description",
                Price = 100,
                Images = null,
                SubCategoryId = 1,
                Condition = Condition.New,
                BidOnly = false,
                SellerId = "seller1"
            };

            _categoryDaoMock.Setup(m => m.GetCategoryByIdAsync(itemDto.SubCategoryId)).ReturnsAsync(new Category { Id = 1, Name = "Category" });
            _userDaoMock.Setup(m => m.GetUserByIdAsync(itemDto.SellerId)).ThrowsAsync(new InvalidOperationException("Seller not found."));

            Assert.ThrowsAsync<InvalidOperationException>(() => _itemManager.CreateItemAsync(itemDto));
        }

        [Test]
        public async Task UpdateItemAsync_ExistingItem_UpdatesItem()
        {
            var item = new Item { Id = 1, Name = "Existing Item", Description = "Old Description", Price = 100 };
            await _itemDaoFake.CreateItemAsync(item);

            var itemDto = new ItemUpdateDTO
            {
                Id = 1,
                Description = "Updated Description",
                Price = 200,
            };

            await _itemManager.UpdateItemAsync(itemDto);

            var updatedItem = await _itemDaoFake.GetItemByIdAsync(itemDto.Id);
            Assert.AreEqual("Updated Description", updatedItem.Description);
            Assert.AreEqual(200, updatedItem.Price);
        }


        [Test]
        public void UpdateItemAsync_ItemDoesNotExist_ThrowsException()
        {
            var itemDto = new ItemUpdateDTO
            {
                Id = 1,
                Description = "Updated Description",
                Price = 200,
            };

            Assert.ThrowsAsync<InvalidOperationException>(() => _itemManager.UpdateItemAsync(itemDto));
        }

        [Test]
        public async Task DeleteItemAsync_ExistingItem_DeletesItem()
        {
            var item = new Item { Id = 1, Name = "Existing Item" };
            await _itemDaoFake.CreateItemAsync(item);

            await _itemManager.DeleteItemAsync(item.Id);

            var deletedItem = await _itemDaoFake.GetItemByIdAsync(item.Id);
            Assert.IsNull(deletedItem);
        }

        [Test]
        public async Task GetItemByIdAsync_ItemExists_ReturnsItem()
        {
            var item = new Item { Id = 1, Name = "Existing Item" };
            await _itemDaoFake.CreateItemAsync(item);

            var result = await _itemManager.GetItemByIdAsync(item.Id);

            Assert.NotNull(result);
            Assert.AreEqual(item.Id, result.Id);
        }

        [Test]
        public async Task GetItemByIdAsync_ItemDoesNotExist_ReturnsNull()
        {
            var result = await _itemManager.GetItemByIdAsync(999);

            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllItemsAsync_ReturnsAllItems()
        {
            var item1 = new Item { Name = "Item 1" };
            var item2 = new Item { Name = "Item 2" };
            await _itemDaoFake.CreateItemAsync(item1);
            await _itemDaoFake.CreateItemAsync(item2);

            var result = await _itemManager.GetAllItemsAsync();

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task NewBidAsync_ValidBid_CreatesBid()
        {
            var item = new Item { Id = 1, Name = "Existing Item", BidOnly = true, CreatedAt = DateTime.UtcNow, BidDuration = BidDuration.SevenDays };
            await _itemDaoFake.CreateItemAsync(item);

            var bidDto = new CreateItemBidDTO { ItemID = 1, Bid = 100 };
            var bidder = new User { UUID = Guid.NewGuid() };

            _userDaoMock.Setup(m => m.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(bidder);

            await _itemManager.NewBidAsync(bidDto, bidder.UUID.ToString());

            var bids = await _itemDaoFake.GetBidsByItemIdAsync(item.Id);
            Assert.AreEqual(1, bids.Count());
        }

        [Test]
        public async Task NewBidAsync_InvalidBid_ThrowsException()
        {
            var item = new Item { Id = 1, Name = "Existing Item", BidOnly = true, CreatedAt = DateTime.UtcNow, BidDuration = BidDuration.SevenDays };
            await _itemDaoFake.CreateItemAsync(item);

            var bidDto = new CreateItemBidDTO { ItemID = 1, Bid = 100 };
            var bidder = new User { UUID = Guid.NewGuid() };

            _userDaoMock.Setup(m => m.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync(bidder);
            await _itemManager.NewBidAsync(bidDto, bidder.UUID.ToString());

            var newBidDto = new CreateItemBidDTO { ItemID = 1, Bid = 50 };

            Assert.ThrowsAsync<InvalidOperationException>(() => _itemManager.NewBidAsync(newBidDto, bidder.UUID.ToString()));
        }
    }
}
