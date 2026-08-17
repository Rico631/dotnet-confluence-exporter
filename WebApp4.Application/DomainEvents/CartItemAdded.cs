using WebApp4.Application.Common;

namespace WebApp4.Application.DomainEvents;

/// <summary>
/// Событие добавления товара в корзину.
/// </summary>
public record CartItemAdded : IDomainEvent
{
    public Guid UserId { get; set; }
    public Guid CartId { get; set; }
    public Guid CartItemId { get; set; }
}

