using Microsoft.EntityFrameworkCore;
using SignalRMvcDemo.Models;

namespace SignalRMvcDemo.Data;

public class ApplicationDbContext : DbContext
{

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<ChatMessage> ChatMessages { get; set; }
}