using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs.CategoryDTOs;

public class SubcategoryDTO
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Name { get; set; }
    public int ParentCategoryId { get; set; }
}