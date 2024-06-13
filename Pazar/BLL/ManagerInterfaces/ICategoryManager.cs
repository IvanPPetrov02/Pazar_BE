using BLL.Category_related;
using BLL.DTOs.CategoryDTOs;

namespace BLL.CategoryRelated
{
    public interface ICategoryManager
    {
        Task<Category> GetCategoryByIdAsync(int id);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<IEnumerable<Category>> GetAllSubCategoriesAsync();
        Task<Category> CreateCategoryAsync(CategoryCreateDTO categoryDto);
        Task UpdateCategoryAsync(int id, CategoryUpdateDTO categoryDto);
        Task DeleteCategoryAsync(int id);
        Task<IEnumerable<CategoryWithSubcategoriesDTO>> GetAllCategoriesWithSubcategoriesAsync();
        Task<IEnumerable<Category>> GetRandomSubCategoriesAsync(int count);
    }
}