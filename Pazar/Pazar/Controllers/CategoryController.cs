﻿using BLL.CategoryRelated;
using BLL.DTOs.CategoryDTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BLL.Services;

namespace Pazar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryManager _categoryManager;
        private readonly IJwtService _jwtService;

        public CategoryController(ICategoryManager categoryManager, IJwtService jwtService)
        {
            _categoryManager = categoryManager;
            _jwtService = jwtService;
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

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryDto)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

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

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDTO categoryDto)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

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

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString();
            string jwt = null;

            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                jwt = authorizationHeader.Substring("Bearer ".Length).Trim();
            }

            if (string.IsNullOrEmpty(jwt) || !_jwtService.ValidateToken(jwt, out ClaimsPrincipal? principal))
            {
                return Unauthorized(new { message = "Invalid or missing authorization token." });
            }

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
