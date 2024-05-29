using BLL.Category_related;
using BLL.Item_related;

namespace BLL.DTOs.ItemDTOs;

public class ItemUpdateDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double? Price { get; set; }
    public List<ItemImages> Images { get; set; }
    public Category SubCategory { get; set; }
    public Condition Condition { get; set; }
    public bool BidOnly { get; set; }
    public BidDuration? BidDuration { get; set; }
    public ItemStatus Status { get; set; }
    public User Buyer { get; set; }
}