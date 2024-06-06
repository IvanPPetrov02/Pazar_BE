using BLL.Category_related;

namespace BLL.DTOs.CategoryDTOs;

public class CategoryCreateDTO
{
    public string Name { get; set; }
    public int? ParentCategoryId { get; set; }
}