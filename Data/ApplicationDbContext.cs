using Microsoft.EntityFrameworkCore;
using SignalRMvcDemo.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace SignalRMvcDemo.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<ChatMessage> ChatMessages { get; set; }
}