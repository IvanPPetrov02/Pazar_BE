using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BLL.Item_related;

public class ItemBids
{
    [Key]
    public int BidId { get; set; }
    public Item Item { get; set; }
    public User Bidder { get; set; }

    public double Bid { get; set; }
    public DateTime BidTime { get; set; }

    public ItemBids(int bidId, Item item, User bidder, double bid, DateTime bidTime)
    {
        BidId = bidId;
        Item = item;
        Bidder = bidder;
        Bid = bid;
        BidTime = bidTime;
    }

    public ItemBids()
    {
    }
}