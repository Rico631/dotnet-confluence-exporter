
using Microsoft.EntityFrameworkCore;
using WebApp4.Application.Entities;

namespace WebApp4.Application.Infrastructure;

/// <inheritdoc/>
public class CartRepository(CartContext context) : ICartRepository
{
    /// <inheritdoc/>
    public async Task<Cart> AddItemToCart(Guid userId, CartItem cartItem, CancellationToken cancellationToken)
    {
        var cart = context.Carts.Local.FirstOrDefault(x => x.UserId == userId)
            ?? await context.Carts.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart == null)
        {
            cart = new Cart(userId);

            context.Carts.Add(cart);
        }

        cart.AddCartItem(cartItem);

        return cart;
    }
}

