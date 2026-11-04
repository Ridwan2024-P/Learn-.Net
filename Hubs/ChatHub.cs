using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
  
  await Clients.All.SendCoreAsync("ReceiveMessage", new object[] { user, message, chatMessage.SentAt });
 }

 public async Task GetMessages()
 {
  var messages = await _context.ChatMessages.OrderByDescending(m => m.SentAt).ToListAsync();
  await Clients.Caller.SendCoreAsync("LoadMessages",new object[] {messages} );
 }
 public async Task Typing (string user){
  await Clients.Others.SendCoreAsync("Typing", new object[] { user });}
}