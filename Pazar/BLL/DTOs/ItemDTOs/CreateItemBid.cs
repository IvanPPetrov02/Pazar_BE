namespace BLL.DTOs.ItemDTOs;

public class CreateItemBid
{
    public int BidID { get; set; }
    public int ItemID { get; set; }
    public string BidderID { get; set; }
    public double Bid { get; set; }
    public DateTime BidTime { get; set; }
}