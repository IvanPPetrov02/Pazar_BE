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
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Item?> GetItemByIdAsync(int id)
        {
            return await _context.Items
                .Include(i => i.SubCategory)
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Item>> GetAllItemsAsync()
        {
            return await _context.Items
                .Include(i => i.SubCategory)
                .Include(i => i.Seller)
                .Include(i => i.Buyer)
                .ToListAsync();
        }
    }
}