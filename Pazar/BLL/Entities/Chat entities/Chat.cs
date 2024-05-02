using System.ComponentModel.DataAnnotations;

namespace BLL.Chat_related;


public class Chat
{
    [Key]
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