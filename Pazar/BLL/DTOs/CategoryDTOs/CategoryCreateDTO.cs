using BLL.Category_related;

namespace BLL.DTOs.CategoryDTOs;

public class CategoryCreateDTO
{
    public string Name { get; set; }
    public Category? ParentCategory { get; set; }
}