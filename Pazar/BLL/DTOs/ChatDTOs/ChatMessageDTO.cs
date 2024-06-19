namespace BLL.DTOs;

public class ChatMessageDTO
{
    public int ChatId { get; set; }
    public int ItemSoldId { get; set; }
    public string BuyerId { get; set; }
    public string SenderId { get; set; }
    public string MessageSent { get; set; }
}