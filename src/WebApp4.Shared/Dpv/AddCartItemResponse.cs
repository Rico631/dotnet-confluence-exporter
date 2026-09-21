namespace WebApp4.Shared.Dpv;

/// <summary>
/// Документы были сохранены
/// </summary>
public class AddCartItemResponse
{
    /// <summary>
    /// Идентификатор корзины
    /// </summary>
    public Guid CartId { get; set; }

    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public Guid CartItemId { get; set; }

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
