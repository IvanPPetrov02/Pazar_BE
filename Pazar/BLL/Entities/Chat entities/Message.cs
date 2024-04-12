namespace BLL.Chat_related;

public class Message
{
    public int Id { get; set; }
    public User Sender { get; set; }
    public User Receiver { get; set; }
    public DateTime SentAt { get; set; }
    public string? MessageSent { get; set; }
    public List<byte[]>? ImageSent { get; set; }

    public Message(int id, User sender, User receiver, DateTime sentAt, string? messageSent, List<byte[]>? imageSent)
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