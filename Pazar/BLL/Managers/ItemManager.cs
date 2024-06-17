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
        
        private int GetBidDurationInHours(BidDuration bidDuration)
        {
            switch (bidDuration)
            {
                case BidDuration.OneDay:
                    return 24;
                case BidDuration.ThreeDays:
                    return 72;
                case BidDuration.FiveDays:
                    return 120;
                case BidDuration.SevenDays:
                    return 168;
                case BidDuration.FourteenDays:
                    return 336;
                case BidDuration.TwentyOneDays:
                    return 504;
                case BidDuration.ThirtyDays:
                    return 720;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bidDuration), bidDuration, null);
            }
        }

        
        public async Task<IEnumerable<Item>> GetAllItemsFilteredAsync()
        {
            var allItems = await _itemDao.GetAllItemsAsync();
            var filteredItems = new List<Item>();

            foreach (var item in allItems)
            {
                bool isNonBidItem = !item.BidOnly;
                bool isBiddableItem = item.BidOnly && item.CreatedAt.AddHours(GetBidDurationInHours(item.BidDuration.Value)) > DateTime.UtcNow;

                Console.WriteLine($"Item ID: {item.Id}, BidOnly: {item.BidOnly}, CreatedAt: {item.CreatedAt}, BidDuration: {item.BidDuration}");
                Console.WriteLine($"isNonBidItem: {isNonBidItem}, isBiddableItem: {isBiddableItem}");

                if (isNonBidItem || isBiddableItem)
                {
                    filteredItems.Add(item);
                }
            }

            return filteredItems;
        }


        
        public async Task<IEnumerable<Item>> GetItemsBySubCategoryAsync(int subCategoryId)
        {
            var allItems = await GetAllItemsFilteredAsync();

            var itemsInSameSubCategory = allItems.Where(item => item.SubCategory?.Id == subCategoryId);

            return itemsInSameSubCategory;
        }

        public async Task<IEnumerable<Item>> GetItemsBySellerAsync(string sellerId)
        {
            var allItems = await _itemDao.GetAllItemsAsync();

            var filteredItems = allItems.Where(item => item.Seller.UUID.ToString() == sellerId);

            return filteredItems;
        }

        public async Task NewBidAsync(CreateItemBidDTO bidDto, string bidderId)
        {
            try
            {
                var item = await _itemDao.GetItemByIdAsync(bidDto.ItemID);
                if (item == null) throw new InvalidOperationException("Item not found.");

                var highestBid = (await _itemDao.GetBidsByItemIdAsync(bidDto.ItemID)).OrderByDescending(b => b.Bid).FirstOrDefault();
                if (highestBid != null && bidDto.Bid <= highestBid.Bid)
                {
                    throw new InvalidOperationException("New bid must be higher than the current highest bid.");
                }

                var bidder = await _userDao.GetUserByIdAsync(bidderId);
                if (bidder == null) throw new InvalidOperationException("Bidder not found.");

                ItemBids newBid = new ItemBids
                {
                    Bid = bidDto.Bid,
                    BidTime = DateTime.UtcNow,
                    Bidder = bidder,
                    Item = item
                };

                await _itemDao.NewBidAsync(newBid);
                Console.WriteLine("New bid successfully placed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error placing new bid: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public async Task<IEnumerable<ItemBids>> GetBidsByItemIdAsync(int itemId)
        {
            return await _itemDao.GetBidsByItemIdAsync(itemId);
        }
    }
}
