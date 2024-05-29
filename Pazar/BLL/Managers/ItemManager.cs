using BLL.DTOs;
using BLL.Item_related;
using BLL.ManagerInterfaces;
using BLL.RepositoryInterfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.DTOs.ItemDTOs;

namespace BLL
{
    public class ItemManager : IItemManager
    {
        private readonly IItemDAO _itemDao;

        public ItemManager(IItemDAO itemDao)
        {
            _itemDao = itemDao;
        }

        public async Task CreateItemAsync(ItemCreateDTO itemDto)
        {
            var item = new Item
            {
                Name = itemDto.Name,
                Description = itemDto.Description,
                Price = itemDto.Price,
                Images = itemDto.Images,
                SubCategory = itemDto.SubCategory,
                Condition = itemDto.Condition,
                BidOnly = itemDto.BidOnly,
                BidDuration = itemDto.BidDuration,
                Seller = itemDto.Seller,
                CreatedAt = DateTime.UtcNow,
                Status = ItemStatus.Available
            };
            await _itemDao.CreateItemAsync(item);
        }

        public async Task UpdateItemAsync(ItemUpdateDTO itemDto)
        {
            var item = await _itemDao.GetItemByIdAsync(itemDto.Id);
            if (item != null)
            {
                item.Name = itemDto.Name;
                item.Description = itemDto.Description;
                item.Price = itemDto.Price;
                item.Images = itemDto.Images;
                item.SubCategory = itemDto.SubCategory;
                item.Condition = itemDto.Condition;
                item.BidOnly = itemDto.BidOnly;
                item.BidDuration = itemDto.BidDuration;
                item.Status = itemDto.Status;
                item.Buyer = itemDto.Buyer;

                await _itemDao.UpdateItemAsync(item);
            }
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
            if (item != null)
            {
                item.Status = status;
                await _itemDao.UpdateItemAsync(item);
            }
        }

        public async Task UpdateItemImagesAsync(int id, List<ItemImages> images)
        {
            var item = await _itemDao.GetItemByIdAsync(id);
            if (item != null)
            {
                item.Images = images;
                await _itemDao.UpdateItemAsync(item);
            }
        }
    }
}
