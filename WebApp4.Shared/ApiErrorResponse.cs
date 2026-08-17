namespace WebApp4.Shared;

/// <summary>
/// Ошибка
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
