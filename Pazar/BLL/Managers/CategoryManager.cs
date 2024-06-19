using BLL.Category_related;
using BLL.DTOs.CategoryDTOs;
using BLL.RepositoryInterfaces;

namespace BLL.CategoryRelated
{
    public class CategoryManager : ICategoryManager
    {
        private readonly ICategoryDAO _categoryDao;

        public CategoryManager(ICategoryDAO categoryDao)
        {
            _categoryDao = categoryDao;
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryDao.GetCategoryByIdAsync(id);
            if (category == null) throw new InvalidOperationException("Category not found.");
            return category;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            var allCategories = await _categoryDao.GetAllCategoriesAsync();
            return allCategories.Where(c => c.ParentCategory == null);
        }

        public async Task<IEnumerable<Category>> GetAllSubCategoriesAsync()
        {
            var allCategories = await _categoryDao.GetAllCategoriesAsync();
            return allCategories.Where(c => c.ParentCategory != null);
        }

        public async Task<Category> CreateCategoryAsync(CategoryCreateDTO categoryDto)
        {
            var parentCategory = categoryDto.ParentCategoryId.HasValue 
                ? await _categoryDao.GetCategoryByIdAsync(categoryDto.ParentCategoryId.Value) 
                : null;

            var category = new Category
            {
                Name = categoryDto.Name,
                ParentCategory = parentCategory
            };

            await _categoryDao.CreateCategoryAsync(category);
            return category;
        }

        public async Task UpdateCategoryAsync(int id, CategoryUpdateDTO categoryDto)
        {
            var category = await _categoryDao.GetCategoryByIdAsync(id);
            if (category == null) throw new InvalidOperationException("Category not found.");

            category.Name = categoryDto.Name;

            var parentCategory = categoryDto.ParentCategoryId.HasValue
                ? await _categoryDao.GetCategoryByIdAsync(categoryDto.ParentCategoryId.Value)
                : null;

            category.ParentCategory = parentCategory;

            await _categoryDao.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            var category = await _categoryDao.GetCategoryByIdAsync(id);
            if (category == null) throw new InvalidOperationException("Category not found.");
    
            if (category.ParentCategory == null)
            {
                var allCategories = await _categoryDao.GetAllCategoriesAsync();
                var subcategoryIds = allCategories
                    .Where(c => c.ParentCategory != null && c.ParentCategory.Id == category.Id)
                    .Select(c => c.Id)
                    .ToList();

                foreach (var subcategoryId in subcategoryIds)
                {
                    await _categoryDao.DeleteCategoryAsync(subcategoryId);
                }
            }
    
            await _categoryDao.DeleteCategoryAsync(id);
        }

        
        public async Task<IEnumerable<CategoryWithSubcategoriesDTO>> GetAllCategoriesWithSubcategoriesAsync()
        {
            var allCategories = await _categoryDao.GetAllCategoriesAsync();
            var mainCategories = allCategories.Where(c => c.ParentCategory == null);

            var result = new List<CategoryWithSubcategoriesDTO>();

            foreach (var mainCategory in mainCategories)
            {
                var subcategories = allCategories
                    .Where(c => c.ParentCategory != null && c.ParentCategory.Id == mainCategory.Id)
                    .Select(c => new SubcategoryDTO { Id = c.Id, Name = c.Name })
                    .ToList();

                result.Add(new CategoryWithSubcategoriesDTO
                {
                    Id = mainCategory.Id,
                    Name = mainCategory.Name,
                    Subcategories = subcategories
                });
            }

            return result;
        }
        
        public async Task<IEnumerable<Category>> GetRandomSubCategoriesAsync(int count)
        {
            var allSubCategories = (await _categoryDao.GetAllCategoriesAsync()).Where(c => c.ParentCategory != null).ToList();
            var random = new Random();
            return allSubCategories.OrderBy(x => random.Next()).Take(count);
        }
    }
}