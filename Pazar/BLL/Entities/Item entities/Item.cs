using System.ComponentModel.DataAnnotations;
using BLL.Category_related;

namespace BLL.Item_related;

public class Item
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public double? Price { get; set; }
    public List<ItemImages> Images { get; set; }
    public Category SubCategory { get; set; }
    public Condition Condition { get; set; }
    public bool BidOnly { get; set; }
    public ItemStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public BidDuration? BidDuration { get; set; }
    public User Seller { get; set; }
    public User? Buyer { get; set; }

    public Item(int id, string name, string description, double? price, List<ItemImages> images, Category subCategory, Condition condition, bool bidOnly, ItemStatus status, DateTime createdAt, BidDuration? bidDuration, User seller, User? buyer)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        Images = images;
        SubCategory = subCategory;
        Condition = condition;
        BidOnly = bidOnly;
        Status = status;
        CreatedAt = createdAt;
        BidDuration = bidDuration;
        Seller = seller;
        Buyer = buyer;
    }

    public Item()
    {
    }
}