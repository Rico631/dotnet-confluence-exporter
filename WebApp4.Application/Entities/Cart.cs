
using WebApp4.Application.Common;
using WebApp4.Application.DomainEvents;

namespace WebApp4.Application.Entities;

/// <summary>
/// Корзина покупок
/// </summary>
public class Cart : Entity
{
    private Cart()
    {

    }
    public Cart(Guid userId)
    {
        UserId = userId;
    }

    /// <summary>
    /// Идентификатор пользователя, которому принадлежит корзина
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Список товаров в корзине
    /// </summary>
    public List<CartItem> CartItems { get; set; } = [];

    internal void AddCartItem(CartItem cartItem)
    {
        CartItems.Add(cartItem);

        Raise(new CartItemAdded
        {
            UserId = UserId,
            CartId = Id,
            CartItemId = cartItem.Id,
        });
    }
}

