
using Microsoft.EntityFrameworkCore;
using WebApp4.Application.Entities;

namespace WebApp4.Application;

public class UserContext : DbContext
{

    public DbSet<User> Users { get; set; }

    public DbSet<UserDocument> UserDocuments { get; set; }

    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add your entity configurations here
    }
}