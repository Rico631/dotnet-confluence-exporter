using WebApp4.Application.Common;

namespace WebApp4.Application.Entities;

/// <summary>
/// Элемент корзины покупок
/// </summary>
public class CartItem : Entity
{

    private CartItem()
    {

    }

    public CartItem(string name, int quantity, decimal price)
    {
        Name = name;
        Quantity = quantity;
        Price = price;
    }

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
