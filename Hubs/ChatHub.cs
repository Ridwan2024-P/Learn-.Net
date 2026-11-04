using Microsoft.AspNetCore.SignalR;
using SignalRMvcDemo.Data;
using SignalRMvcDemo.Models;

namespace SignalRMvcDemo.Hubs;

public class ChatHub :Hub
{
 private readonly ApplicationDbContext _context;

 public ChatHub(ApplicationDbContext context)
 {
  _context = context;
 }
 public async Task sendMessage(string user, string message)
 {
  var chatMessage = new ChatMessage
  {
   User = user,
   Message = message,
   SentAt = DateTime.UtcNow
  };
  _context.ChatMessages.Add(chatMessage);
  await _context.SaveChangesAsync();
  
  await Clients.All.SendCoreAsync("ReceiveMessage", new object[] { user, message });
 }
}