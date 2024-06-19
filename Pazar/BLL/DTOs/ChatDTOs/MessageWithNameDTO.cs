namespace BLL.DTOs;

public class MessageWithNameDTO
{
    public int Id { get; set; }
    public string MessageSent { get; set; }
    public DateTime SentAt { get; set; }
    public string SenderName { get; set; }
    public string SenderId { get; set; }
}