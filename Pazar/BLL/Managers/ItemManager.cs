using BLL.DTOs.ItemDTOs;
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
                Description = itemDto.Description.Length > 300 ? itemDto.Description.Substring(0, 300) : itemDto.Description,
                Price = itemDto.BidOnly ? null : itemDto.Price,
                Images = itemDto.Images?.Select(img => new ItemImages { Image = null }).ToList(), // Set image to null
                SubCategory = subCategory,
                Condition = itemDto.Condition,
                BidOnly = itemDto.BidOnly,
                Status = itemDto.BidOnly ? ItemStatus.Bidding : ItemStatus.Available,
                CreatedAt = DateTime.UtcNow,
                BidDuration = itemDto.BidOnly ? itemDto.BidDuration : null,
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
            item.Description = itemDto.Description.Length > 300
                ? itemDto.Description.Substring(0, 300)
                : itemDto.Description;
            item.Price = itemDto.Price;
            item.Images = itemDto.Images?.Select(img => new ItemImages { Image = null }).ToList(); // Set image to null
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
        
        public async Task<bool> IsUserSellerAsync(int itemId, string userId)
        {
            var item = await _itemDao.GetItemByIdAsync(itemId);
            if (item == null) throw new InvalidOperationException("Item not found.");

            return item.Seller.UUID.ToString() == userId;
        }
        
        public async Task<IEnumerable<Item>> GetItemsByParentCategoryAsync(int parentCategoryId)
        {
            var allItems = await GetAllItemsFilteredAsync();
    
            var itemsInSameParentCategory = allItems.Where(item => item.SubCategory?.ParentCategory?.Id == parentCategoryId);

            return itemsInSameParentCategory;
        }
        
        public async Task<IEnumerable<Item>> GetAllItemsFilteredAsync()
        {
            var allItems = await _itemDao.GetAllItemsAsync();

            var filteredItems = allItems.Where(item =>
                !item.BidOnly ||
                item.CreatedAt.AddHours((double)item.BidDuration) > DateTime.UtcNow);

            return filteredItems;
        }
    }
}
