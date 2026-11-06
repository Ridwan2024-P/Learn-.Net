namespace SignalRMvcDemo.Models;
using System.ComponentModel.DataAnnotations;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    [Required]
    public String User { get; set; } = string.Empty; 
    
    public string ? SenderId { get; set; }
    public string ?  ReceiverId { get; set; } 
    [Required]
    public String Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;
}