﻿using BLL.Category_related;
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

        public async Task<Category> CreateCategoryAsync(CategoryCreateDTO categoryDto) // Updated return type
        {
            var parentCategory = categoryDto.ParentCategory != null
                ? await _categoryDao.GetCategoryByIdAsync(categoryDto.ParentCategory.Id)
                : null;

            var category = new Category
            {
                Name = categoryDto.Name,
                ParentCategory = parentCategory
            };

            await _categoryDao.CreateCategoryAsync(category);
            return category; // Return the created category
        }

        public async Task UpdateCategoryAsync(int id, CategoryUpdateDTO categoryDto)
        {
            var category = await _categoryDao.GetCategoryByIdAsync(id);
            if (category == null) throw new InvalidOperationException("Category not found.");

            category.Name = categoryDto.Name;
            category.ParentCategory = categoryDto.ParentCategory;

            await _categoryDao.UpdateCategoryAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
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
    }
}