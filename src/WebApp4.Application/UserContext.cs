
using Microsoft.EntityFrameworkCore;
using WebApp4.Application.Entities;

namespace WebApp4.Application;

public class UserContext(DbContextOptions<UserContext> options) : DbContext(options)
{

    public DbSet<User> Users { get; set; }

    public DbSet<UserDocument> UserDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add your entity configurations here
    }
}