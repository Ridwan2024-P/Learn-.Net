using Microsoft.AspNetCore.SignalR;

namespace SignalRMvcDemo.Hubs;

public class ChatHub :Hub
{
 public async Task sendMessage(string user, string message)
 {
  await Clients.All.SendCoreAsync("ReceiveMessage", new object[] { user, message });
 }
}