using BLL.Item_related;
using BLL.RepositoryInterfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests.FakeDAL
{
    public class ItemDAOFake : IItemDAO
    {
        private readonly List<Item> _items = new List<Item>();
        private readonly List<ItemBids> _bids = new List<ItemBids>();

        public Task<Item> GetItemByIdAsync(int id)
        {
            return Task.FromResult(_items.FirstOrDefault(i => i.Id == id));
        }

        public Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return Task.FromResult((IEnumerable<Item>)_items);
        }

        public Task CreateItemAsync(Item item)
        {
            item.Id = _items.Count + 1;
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task UpdateItemAsync(Item item)
        {
            var existingItem = _items.FirstOrDefault(i => i.Id == item.Id);
            if (existingItem != null)
            {
                existingItem.Name = item.Name;
                existingItem.Description = item.Description;
                existingItem.Price = item.Price;
                existingItem.Images = item.Images;
                existingItem.SubCategory = item.SubCategory;
                existingItem.Condition = item.Condition;
                existingItem.BidOnly = item.BidOnly;
                existingItem.Status = item.Status;
                existingItem.BidDuration = item.BidDuration;
                existingItem.Seller = item.Seller;
                existingItem.Buyer = item.Buyer;
            }
            return Task.CompletedTask;
        }

        public Task DeleteItemAsync(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
            return Task.CompletedTask;
        }

        public Task NewBidAsync(ItemBids bid)
        {
            _bids.Add(bid);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ItemBids>> GetBidsByItemIdAsync(int itemId)
        {
            return Task.FromResult(_bids.Where(b => b.Item.Id == itemId).AsEnumerable());
        }
    }
}
