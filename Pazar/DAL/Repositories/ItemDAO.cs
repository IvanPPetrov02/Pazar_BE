using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Item_related;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories
{
    public class ItemDAO : IItemDAO
    {
        private readonly AppDbContext _context;

        public ItemDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            return await _context.Items
                .Include(i => i.Images)
                .Include(i => i.SubCategory)
                .ThenInclude(s => s.ParentCategory)
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await _context.Items
                .Include(i => i.Images)
                .Include(i => i.SubCategory)
                .ThenInclude(s => s.ParentCategory)
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .ToListAsync();
        }

        public async Task CreateItemAsync(Item item)
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateItemAsync(Item item)
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteItemAsync(int id)
        {
            var item = await GetItemByIdAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task NewBidAsync(ItemBids bid)
        {
            _context.ItemBids.Add(bid);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ItemBids>> GetBidsByItemIdAsync(int itemId)
        {
            return await _context.ItemBids
                .Include(b => b.Bidder)
                .Where(b => b.Item.Id == itemId)
                .ToListAsync();
        }
    }
}