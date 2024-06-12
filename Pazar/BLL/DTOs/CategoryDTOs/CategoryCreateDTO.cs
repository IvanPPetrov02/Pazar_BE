using System.ComponentModel.DataAnnotations;
using BLL.Category_related;

namespace BLL.DTOs.CategoryDTOs;

public class CategoryCreateDTO
{
    [MaxLength(100)]
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
}