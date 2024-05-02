using System.ComponentModel.DataAnnotations;

namespace BLL.Chat_related;

public class Message
{
    [Key]
    public int Id { get; set; }
    public User Sender { get; set; }
    public User Receiver { get; set; }
    public DateTime SentAt { get; set; }
    public string? MessageSent { get; set; }
    public byte[]? ImageSent { get; set; }

    public Message(int id, User sender, User receiver, DateTime sentAt, string? messageSent, byte[] imageSent)
    {
        Id = id;
        Sender = sender;
        Receiver = receiver;
        SentAt = sentAt;
        MessageSent = messageSent;
        ImageSent = imageSent;
    }

    public Message()
    {
    }
}