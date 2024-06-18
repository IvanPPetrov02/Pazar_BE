namespace BLL.DTOs
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public int ItemSoldId { get; set; }
        public string BuyerId { get; set; }
        public string SellerId { get; set; }  // Add SellerId
    }
}