namespace BLL.Chat_related;


public class Chat
{
    public int Id { get; set; }
    public Message[] Messages { get; set; }

    public Chat(int id, Message[] messages)
    {
        Id = id;
        Messages = messages;
    }

    public Chat()
    {
    }
}