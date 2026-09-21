using Microsoft.EntityFrameworkCore;
using WebApp4.Application.Entities;

namespace WebApp4.Application;

public class CartContext(DbContextOptions<CartContext> options) : DbContext(options)
{

    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Add your entity configurations here
    }
}
