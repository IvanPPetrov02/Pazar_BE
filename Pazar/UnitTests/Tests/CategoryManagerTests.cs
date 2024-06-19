using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLL.Category_related;
using BLL.CategoryRelated;
using BLL.DTOs.CategoryDTOs;
using BLL.Managers;
using NUnit.Framework;
using UnitTests.FakeDAL;

namespace UnitTests
{
    [TestFixture]
    public class CategoryManagerTests
    {
        private CategoryManager _categoryManager;
        private CategoryDAOFake _categoryDaoFake;

        [SetUp]
        public void Setup()
        {
            _categoryDaoFake = new CategoryDAOFake();
            _categoryManager = new CategoryManager(_categoryDaoFake);
        }

        [Test]
        public async Task GetCategoryByIdAsync_CategoryExists_ReturnsCategory()
        {
            var category = new Category { Name = "Test Category" };
            await _categoryDaoFake.CreateCategoryAsync(category);

            var result = await _categoryManager.GetCategoryByIdAsync(category.Id);

            Assert.NotNull(result);
            Assert.AreEqual(category.Name, result.Name);
        }

        [Test]
        public void GetCategoryByIdAsync_CategoryDoesNotExist_ThrowsInvalidOperationException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _categoryManager.GetCategoryByIdAsync(999));
        }

        [Test]
        public async Task GetAllCategoriesAsync_ReturnsAllMainCategories()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory = new Category { Name = "Sub Category", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory);

            var result = await _categoryManager.GetAllCategoriesAsync();

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual("Main Category", result.First().Name);
        }

        [Test]
        public async Task GetAllSubCategoriesAsync_ReturnsAllSubCategories()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory1 = new Category { Name = "Sub Category 1", ParentCategory = mainCategory };
            var subCategory2 = new Category { Name = "Sub Category 2", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory1);
            await _categoryDaoFake.CreateCategoryAsync(subCategory2);

            var result = await _categoryManager.GetAllSubCategoriesAsync();

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [Test]
        public async Task CreateCategoryAsync_ValidCategory_ReturnsCreatedCategory()
        {
            var categoryDto = new CategoryCreateDTO { Name = "New Category" };

            var result = await _categoryManager.CreateCategoryAsync(categoryDto);

            Assert.NotNull(result);
            Assert.AreEqual(categoryDto.Name, result.Name);
        }
        
        [Test]
        public async Task UpdateCategoryAsync_ExistingCategory_UpdatesCategory()
        {
            var category = new Category { Name = "Existing Category" };
            await _categoryDaoFake.CreateCategoryAsync(category);
            var updateDto = new CategoryUpdateDTO { Name = "Updated Category" };

            await _categoryManager.UpdateCategoryAsync(category.Id, updateDto);
            var updatedCategory = await _categoryDaoFake.GetCategoryByIdAsync(category.Id);

            Assert.AreEqual(updateDto.Name, updatedCategory.Name);
        }

        [Test]
        public async Task UpdateCategoryAsync_CategoryDoesNotExist_ThrowsInvalidOperationException()
        {
            var updateDto = new CategoryUpdateDTO { Name = "Updated Category" };

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _categoryManager.UpdateCategoryAsync(999, updateDto));
        }
        
        [Test]
        public async Task DeleteCategoryAsync_ParentCategory_DeletesCategoryAndSubcategories()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory = new Category { Name = "Sub Category", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory);

            await _categoryManager.DeleteCategoryAsync(mainCategory.Id);

            var allCategories = await _categoryDaoFake.GetAllCategoriesAsync();

            Assert.IsEmpty(allCategories);
        }

        [Test]
        public async Task DeleteCategoryAsync_SubCategory_DeletesOnlySubCategory()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory1 = new Category { Name = "Sub Category 1", ParentCategory = mainCategory };
            var subCategory2 = new Category { Name = "Sub Category 2", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory1);
            await _categoryDaoFake.CreateCategoryAsync(subCategory2);

            await _categoryManager.DeleteCategoryAsync(subCategory1.Id);

            var allCategories = await _categoryDaoFake.GetAllCategoriesAsync();

            Assert.AreEqual(2, allCategories.Count());
            Assert.IsTrue(allCategories.Any(c => c.Id == mainCategory.Id));
            Assert.IsTrue(allCategories.Any(c => c.Id == subCategory2.Id));
        }

        [Test]
        public async Task DeleteCategoryAsync_CategoryDoesNotExist_ThrowsInvalidOperationException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _categoryManager.DeleteCategoryAsync(999));
        }

        [Test]
        public async Task GetAllCategoriesWithSubcategoriesAsync_ReturnsCategoriesWithSubcategories()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory1 = new Category { Name = "Sub Category 1", ParentCategory = mainCategory };
            var subCategory2 = new Category { Name = "Sub Category 2", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory1);
            await _categoryDaoFake.CreateCategoryAsync(subCategory2);

            var result = await _categoryManager.GetAllCategoriesWithSubcategoriesAsync();

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(2, result.First().Subcategories.Count());
        }

        [Test]
        public async Task GetRandomSubCategoriesAsync_ReturnsRandomSubCategories()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory1 = new Category { Name = "Sub Category 1", ParentCategory = mainCategory };
            var subCategory2 = new Category { Name = "Sub Category 2", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory1);
            await _categoryDaoFake.CreateCategoryAsync(subCategory2);

            var result = await _categoryManager.GetRandomSubCategoriesAsync(1);

            Assert.NotNull(result);
            Assert.AreEqual(1, result.Count());
        }

        [Test]
        public async Task GetRandomSubCategoriesAsync_CountGreaterThanAvailable_ReturnsAllSubCategories()
        {
            var mainCategory = new Category { Name = "Main Category" };
            var subCategory1 = new Category { Name = "Sub Category 1", ParentCategory = mainCategory };
            var subCategory2 = new Category { Name = "Sub Category 2", ParentCategory = mainCategory };
            await _categoryDaoFake.CreateCategoryAsync(mainCategory);
            await _categoryDaoFake.CreateCategoryAsync(subCategory1);
            await _categoryDaoFake.CreateCategoryAsync(subCategory2);

            var result = await _categoryManager.GetRandomSubCategoriesAsync(10);

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count());
        }
    }
}
