using System.ComponentModel.DataAnnotations;
using BLL.Item_related;

namespace BLL.Chat_related;


public class Chat
{
    [Key]
    public int Id { get; set; }
    public Item ItemSold { get; set; }
    public User Buyer { get; set; }
    
    public Chat(int id, Item itemSold, User buyer)
    {
        Id = id;
        ItemSold = itemSold;
        Buyer = buyer;
    }

    public Chat()
    {
    }
}