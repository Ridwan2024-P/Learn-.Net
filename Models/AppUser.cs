using Microsoft.AspNetCore.Identity;

namespace SignalRMvcDemo.Models;

public class AppUser : IdentityUser
{
    public string FullName { get; set; }
    
}