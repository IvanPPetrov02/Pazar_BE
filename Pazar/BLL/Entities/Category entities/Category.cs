using System.ComponentModel.DataAnnotations;

namespace BLL.Category_related;

public class Category
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public Category? ParentCategory { get; set; }
    
    public Category(int id, string name, Category? parentCategory)
    {
        Id = id;
        Name = name;
        ParentCategory = parentCategory;
    }
    
    public Category()
    {

    }
}