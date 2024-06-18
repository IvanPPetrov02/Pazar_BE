using System.ComponentModel.DataAnnotations;

namespace BLL.Chat_related;

public class Message
{
    [Key]
    public int Id { get; set; }
    public User Sender { get; set; }
    public DateTime SentAt { get; set; }
    public string? MessageSent { get; set; }
    public Chat Chat { get; set; }
    
    public Message(int id, User sender, DateTime sentAt, string messageSent, Chat chat)
    {
        Id = id;
        Sender = sender;
        SentAt = sentAt;
        MessageSent = messageSent;
        Chat = chat;
    }

    public Message()
    {
    }
}