using BLL.RepositoryInterfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Category_related;

public class CategoryDAOFake : ICategoryDAO
{
    private readonly List<Category> _categories = new List<Category>();

    public Task<Category> GetCategoryByIdAsync(int id)
    {
        return Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
    }

    public Task<IEnumerable<Category>> GetAllCategoriesAsync()
    {
        return Task.FromResult((IEnumerable<Category>)_categories);
    }

    public Task CreateCategoryAsync(Category category)
    {
        category.Id = _categories.Count + 1;
        _categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateCategoryAsync(Category category)
    {
        var existingCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
        if (existingCategory != null)
        {
            existingCategory.Name = category.Name;
        }
        return Task.CompletedTask;
    }

    public Task DeleteCategoryAsync(int id)
    {
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category != null)
        {
            _categories.Remove(category);
        }
        return Task.CompletedTask;
    }
}