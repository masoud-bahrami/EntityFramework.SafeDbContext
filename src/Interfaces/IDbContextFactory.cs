using Microsoft.EntityFrameworkCore;

namespace EntityFramework.SafeDbContext
{
    public interface IDbContextFactory
    {
        TDbContext Create<TDbContext>()
            where TDbContext : DbContext;
    }
}