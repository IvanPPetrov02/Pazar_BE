using Microsoft.EntityFrameworkCore;
using BLL;
using BLL.Category_related;
using BLL.Chat_related;
using BLL.Item_related;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DAL.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Messages { get; set; }
    
}