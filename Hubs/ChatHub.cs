
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalRMvcDemo.Data;
using SignalRMvcDemo.Models;

namespace SignalRMvcDemo.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ApplicationDbContext _context;

    public ChatHub(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task SendPrivateMessage(string receiverId, string message)
    {
        var senderId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(senderId))
            throw new HubException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(receiverId))
            throw new HubException("Receiver is required.");

        if (senderId == receiverId)
            throw new HubException("You cannot message yourself.");

        if (string.IsNullOrWhiteSpace(message))
            throw new HubException("Message cannot be empty.");

        message = message.Trim();

        if (message.Length > 5000)
            throw new HubException("Message is too long.");

        var receiverExists = await _context.Users
            .AnyAsync(u => u.Id == receiverId);

        if (!receiverExists)
            throw new HubException("Receiver not found.");

        var chatMessage = new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            User = string.Empty,
            Message = message,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.ChatMessages.Add(chatMessage);

        await _context.SaveChangesAsync();

        var messageData = new
        {
            id = chatMessage.Id,
            senderId = senderId,
            receiverId = receiverId,
            message = chatMessage.Message,
            sentAt = chatMessage.SentAt,
            isRead = chatMessage.IsRead
        };

        
        await Clients.User(receiverId).SendAsync(
            "ReceivePrivateMessage",
            messageData
        );

        
        await Clients.User(senderId).SendAsync(
            "ReceivePrivateMessage",
            messageData
        );
        await SendUnreadCount(receiverId);
        await Clients.Users(senderId, receiverId)
            .SendAsync("ConversationsUpdated");
    }
    public async Task GetPrivateMessages(string otherUserId)
    {
        var currentUserId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(currentUserId))
            throw new HubException("User is not authenticated.");
        if (string.IsNullOrWhiteSpace(otherUserId) ||
            otherUserId == currentUserId)
            throw new HubException("Invalid conversation.");
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == otherUserId);
        if (!userExists)
            throw new HubException("User not found.");
       
        await MarkMessagesAsRead(otherUserId);

        var messages = await _context.ChatMessages
            .Where(m =>
                (m.SenderId == currentUserId &&
                 m.ReceiverId == otherUserId)
                ||
                (m.SenderId == otherUserId &&
                 m.ReceiverId == currentUserId)
            )
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        await Clients.Caller.SendAsync(
            "LoadPrivateMessages",
            messages
        );

      
        await SendUnreadCount(currentUserId);

        
        await Clients.Users(currentUserId, otherUserId)
            .SendAsync("ConversationsUpdated");
    }
    
    public async Task MarkMessagesAsRead(string senderId)
    {
        var currentUserId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(currentUserId))
            throw new HubException("User is not authenticated.");

        if (string.IsNullOrWhiteSpace(senderId) ||
            senderId == currentUserId)
            throw new HubException("Invalid sender.");

        var unreadMessages = await _context.ChatMessages
            .Where(m =>
                m.SenderId == senderId &&
                m.ReceiverId == currentUserId &&
                !m.IsRead)
            .ToListAsync();

        if (unreadMessages.Count == 0)
            return;

        foreach (var msg in unreadMessages)
        {
            msg.IsRead = true;
        }

        await _context.SaveChangesAsync();
        await SendUnreadCount(currentUserId);
        await Clients.User(senderId)
            .SendAsync("ConversationsUpdated");
    }


    public async Task GetUnreadMessageCount()
    {
        var currentUserId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(currentUserId))
            throw new HubException("User is not authenticated.");

        await SendUnreadCount(currentUserId);
    }
    
    private async Task SendUnreadCount(string userId)
    {
        var unreadCount = await _context.ChatMessages
            .CountAsync(m =>
                m.ReceiverId == userId &&
                !m.IsRead);

        await Clients.User(userId).SendAsync(
            "UnreadCountUpdated",
            unreadCount
        );
    }

    
    public async Task GetConversations()
    {
        var currentUserId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(currentUserId))
            throw new HubException("User is not authenticated.");

        var messages = await _context.ChatMessages
            .Where(m =>
                m.SenderId == currentUserId ||
                m.ReceiverId == currentUserId)
            .OrderByDescending(m => m.SentAt)
            .ToListAsync();

        var conversations = messages
            .GroupBy(m =>
                m.SenderId == currentUserId
                    ? m.ReceiverId
                    : m.SenderId)
            .Where(g => !string.IsNullOrEmpty(g.Key))
            .Select(g => new
            {
                UserId = g.Key!,
                LastMessage = g.First(),
                UnreadCount = g.Count(m =>
                    m.ReceiverId == currentUserId &&
                    !m.IsRead)
            })
            .ToList();

        var userIds = conversations
            .Select(c => c.UserId)
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id);

        var result = conversations
            .Where(c => users.ContainsKey(c.UserId))
            .Select(c => new
            {
                userId = c.UserId,
                fullName = users[c.UserId].FullName,
                email = users[c.UserId].Email,
                lastMessage = c.LastMessage.Message,
                lastMessageAt = c.LastMessage.SentAt,
                unreadCount = c.UnreadCount
            })
            .ToList();

        await Clients.Caller.SendAsync(
            "LoadConversations",
            result
        );
    }

    
    public async Task Typing(string receiverId)
    {
        var senderId = Context.UserIdentifier;

        if (string.IsNullOrEmpty(senderId) ||
            string.IsNullOrWhiteSpace(receiverId) ||
            senderId == receiverId)
            return;

        var receiverExists = await _context.Users
            .AnyAsync(u => u.Id == receiverId);

        if (!receiverExists)
            return;

        await Clients.User(receiverId).SendAsync(
            "UserTyping",
            senderId
        );
    }
}
