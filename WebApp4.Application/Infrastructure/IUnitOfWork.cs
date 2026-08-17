namespace WebApp4.Application.Infrastructure;

public interface IUnitOfWork
{
    /// <summary>
    /// Сохранение результата изменений в БД
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}