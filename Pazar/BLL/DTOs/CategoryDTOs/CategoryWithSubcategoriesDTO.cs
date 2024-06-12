using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.CategoryDTOs;

public class CategoryWithSubcategoriesDTO
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    public List<SubcategoryDTO> Subcategories { get; set; }
}