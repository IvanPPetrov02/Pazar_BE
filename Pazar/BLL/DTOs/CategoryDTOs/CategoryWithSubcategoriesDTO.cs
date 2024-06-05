namespace BLL.DTOs.CategoryDTOs;

public class CategoryWithSubcategoriesDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<SubcategoryDTO> Subcategories { get; set; }
}