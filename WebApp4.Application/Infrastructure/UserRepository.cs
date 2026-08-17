using Microsoft.EntityFrameworkCore;
using WebApp4.Application.Entities;

namespace WebApp4.Application.Infrastructure;

/// <inheritdoc/>
public class UserRepository(UserContext context) : IUserRepository
{
    /// <inheritdoc/>
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    }
}

