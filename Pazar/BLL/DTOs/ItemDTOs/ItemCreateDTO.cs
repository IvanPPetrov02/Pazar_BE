using System.ComponentModel.DataAnnotations;
using BLL.Category_related;
using BLL.Item_related;

namespace BLL.DTOs.ItemDTOs;

public class ItemCreateDTO
{
    public string Name { get; set; }
    [MaxLength(300)]
    public string Description { get; set; }
    public double? Price { get; set; }
    public List<ItemImageDTO>? Images { get; set; }
    public int SubCategoryId { get; set; }
    public Condition Condition { get; set; }
    public bool BidOnly { get; set; }
    public ItemStatus Status { get; set; }
    public BidDuration? BidDuration { get; set; }
    public string SellerId { get; set; }
}