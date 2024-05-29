using BLL.Category_related;
using BLL.RepositoryInterfaces;
using DAL.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DAL.CategoryRelated
{
    public class CategoryDAO : ICategoryDAO
    {
        private readonly AppDbContext _context;

        public CategoryDAO(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Category>? GetCategoryByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .ToListAsync();
        }

        public async Task CreateCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}