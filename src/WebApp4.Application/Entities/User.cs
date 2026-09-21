
using WebApp4.Application.Common;

namespace WebApp4.Application.Entities;


/// <summary>
/// Клиент, который может добавлять товары в корзину
/// </summary>
public class User : Entity
{
    public string Name { get; set; } = string.Empty;
}

