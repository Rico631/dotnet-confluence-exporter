using WebApp4.Application.Entities;

namespace WebApp4.Application.Infrastructure;

/// <summary>
/// Хранилище пользователей
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Поиск пользователя по идентификатору
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
}