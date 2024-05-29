using BLL.Category_related;

namespace BLL.RepositoryInterfaces;

public interface ICategoryDAO
{
    Task<Category> GetCategoryByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllCategoriesAsync();
    Task CreateCategoryAsync(Category category);
    Task UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
}