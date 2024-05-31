﻿using BLL.DTOs.ItemDTOs;
using BLL.Item_related;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;

namespace BLL.ItemRelated
{
    public class ItemManager : IItemManager
    {
        private readonly IItemDAO _itemDao;
        private readonly ICategoryDAO _categoryDao;
        private readonly IUserDAO _userDao;

        public ItemManager(IItemDAO itemDao, ICategoryDAO categoryDao, IUserDAO userDao)
        {
            _itemDao = itemDao;
            _categoryDao = categoryDao;
            _userDao = userDao;
        }

        public async Task CreateItemAsync(ItemCreateDTO itemDto)
        {
            var subCategory = await _categoryDao.GetCategoryByIdAsync(itemDto.SubCategoryId);
            var seller = await _userDao.GetUserByIdAsync(itemDto.SellerId);

            var item = new Item
            {
                Name = itemDto.Name,
                Description = itemDto.Description,
                Price = itemDto.Price,
                Images = itemDto.Images?.Select(img => new ItemImages { Image = img.Image }).ToList(),
                SubCategory = subCategory,
                Condition = itemDto.Condition,
                BidOnly = itemDto.BidOnly,
                Status = itemDto.Status,
                CreatedAt = DateTime.UtcNow,
                BidDuration = itemDto.BidDuration,
                Seller = seller,
                Buyer = null
            };

            await _itemDao.CreateItemAsync(item);
        }

        public async Task UpdateItemAsync(ItemUpdateDTO itemDto)
        {
            var item = await _itemDao.GetItemByIdAsync(itemDto.Id);
            if (item == null) throw new InvalidOperationException("Item not found.");

            var subCategory = await _categoryDao.GetCategoryByIdAsync(itemDto.SubCategoryId);
            var buyer = itemDto.BuyerId != null ? await _userDao.GetUserByIdAsync(itemDto.BuyerId) : null;

            item.Name = itemDto.Name;
            item.Description = itemDto.Description;
            item.Price = itemDto.Price;
            item.Images = itemDto.Images?.Select(img => new ItemImages { Image = img.Image }).ToList();
            item.SubCategory = subCategory;
            item.Condition = itemDto.Condition;
            item.BidOnly = itemDto.BidOnly;
            item.Status = itemDto.Status;
            item.BidDuration = itemDto.BidDuration;
            item.Buyer = buyer;

            await _itemDao.UpdateItemAsync(item);
        }

        public async Task DeleteItemAsync(int id)
        {
            await _itemDao.DeleteItemAsync(id);
        }

        public async Task<Item?> GetItemByIdAsync(int id)
        {
            return await _itemDao.GetItemByIdAsync(id);
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await _itemDao.GetAllItemsAsync();
        }

        public async Task UpdateItemStatusAsync(int id, ItemStatus status)
        {
            var item = await _itemDao.GetItemByIdAsync(id);
            if (item == null) throw new InvalidOperationException("Item not found.");

            item.Status = status;
            await _itemDao.UpdateItemAsync(item);
        }

        public async Task UpdateItemImagesAsync(int id, List<ItemImages> images)
        {
            var item = await _itemDao.GetItemByIdAsync(id);
            if (item == null) throw new InvalidOperationException("Item not found.");

            item.Images = images;
            await _itemDao.UpdateItemAsync(item);
        }
    }
}
