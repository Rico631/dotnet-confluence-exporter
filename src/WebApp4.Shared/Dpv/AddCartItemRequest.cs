namespace WebApp4.Shared.Dpv;

/// <summary>
/// Информация о товаре, который нужно добавить в корзину
/// </summary>
public sealed class AddCartItemRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Название товара
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Количество товара
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Цена товара
    /// </summary>
    public decimal Price { get; set; }
}

