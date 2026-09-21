
namespace WebApp4.Application.Infrastructure;

public sealed class UnitOfWork(CartContext cartContext, UserContext userContext) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await Task.WhenAll(
            cartContext.SaveChangesAsync(cancellationToken),
            userContext.SaveChangesAsync(cancellationToken)
            );

    }
}

