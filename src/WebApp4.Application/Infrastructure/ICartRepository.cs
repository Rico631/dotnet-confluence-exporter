using WebApp4.Application.Entities;

namespace WebApp4.Application.Infrastructure;

public interface ICartRepository
{
    /// <summary>
    /// Находит корзину пользователя по его идентификатору. Если корзина не найдена, то создаёт новую корзину.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cartItem"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Cart> AddItemToCart(Guid userId, CartItem cartItem, CancellationToken cancellationToken);
}