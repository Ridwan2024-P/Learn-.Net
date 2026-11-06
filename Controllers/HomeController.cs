
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRMvcDemo.Models;
using SignalRMvcDemo.Data;

namespace SignalRMvcDemo.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;

    public HomeController(
        UserManager<AppUser> userManager,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

 
    [HttpGet]
    public async Task<IActionResult> SearchUsers(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) ||
            searchTerm.Trim().Length < 2)
        {
            return Ok(new List<object>());
        }

        var currentUserId = _userManager.GetUserId(User);
        searchTerm = searchTerm.Trim();

        var users = await _userManager.Users
            .Where(u =>
                u.Id != currentUserId &&
                (
                    (u.UserName != null &&
                     u.UserName.Contains(searchTerm)) ||

                    (u.Email != null &&
                     u.Email.Contains(searchTerm)) ||

                    u.FullName.Contains(searchTerm)
                ))
            .Take(20)
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }

    
    [HttpGet]
    public async Task<IActionResult> GetConversations()
    {
        var currentUserId = _userManager.GetUserId(User);

        if (currentUserId == null)
            return Unauthorized();

       
        var messages = await _context.ChatMessages
            .Where(m =>
                m.SenderId == currentUserId ||
                m.ReceiverId == currentUserId)
            .Select(m => new
            {
                OtherUserId = m.SenderId == currentUserId
                    ? m.ReceiverId
                    : m.SenderId,

                m.Message,
                m.SentAt
            })
            .Where(m => m.OtherUserId != null)
            .ToListAsync();

        
        var latestConversations = messages
            .GroupBy(m => m.OtherUserId)
            .Select(g => g
                .OrderByDescending(m => m.SentAt)
                .First())
            .OrderByDescending(m => m.SentAt)
            .ToList();

        var userIds = latestConversations
            .Select(m => m.OtherUserId!)
            .Distinct()
            .ToList();

     
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new
            {
                id = u.Id,
                fullName = u.FullName,
                email = u.Email
            })
            .ToListAsync();

        
        var conversations = latestConversations
            .Join(
                users,
                message => message.OtherUserId,
                user => user.id,
                (message, user) => new
                {
                    id = user.id,
                    fullName = user.fullName,
                    email = user.email,
                    lastMessage = message.Message,
                    lastMessageTime = message.SentAt
                })
            .ToList();

        return Ok(conversations);
    }
}
