using Microsoft.EntityFrameworkCore;
using WebApp4.Application.Entities;

namespace WebApp4.Application;

public class CartContext : DbContext
{

    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public CartContext(DbContextOptions<CartContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add your entity configurations here
    }
}
