using System;

namespace BLL.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public DateTime SentAt { get; set; }
        public string? MessageSent { get; set; }
        public int ChatId { get; set; }
    }
}