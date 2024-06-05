﻿using BLL.CategoryRelated;
using BLL.DTOs.CategoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pazar.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryManager _categoryManager;

        public CategoryController(ICategoryManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryManager.GetCategoryByIdAsync(id);
                return Ok(category);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet("GetCategories")]
        public async Task<IActionResult> GetAllMainCategories()
        {
            var categories = await _categoryManager.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("GetSubCategories")]
        public async Task<IActionResult> GetAllSubCategories()
        {
            var categories = await _categoryManager.GetAllSubCategoriesAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryDto)
        {
            try
            {
                var createdCategory = await _categoryManager.CreateCategoryAsync(categoryDto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDTO categoryDto)
        {
            try
            {
                await _categoryManager.UpdateCategoryAsync(id, categoryDto);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryManager.DeleteCategoryAsync(id);
                return Ok(new { Message = "Category successfully deleted." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        
        [HttpGet("GetAllCategoriesWithSubcategories")]
        public async Task<IActionResult> GetAllCategoriesWithSubcategories()
        {
            var categories = await _categoryManager.GetAllCategoriesWithSubcategoriesAsync();
            return Ok(categories);
        }

    }
}
