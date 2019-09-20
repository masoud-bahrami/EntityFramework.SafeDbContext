using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFramework.SafeDbContext
{
    public interface IDbContextHandler
    {
        void OnAdd(EntityEntry entityEntry);
        void OnUpdate(EntityEntry entityEntry);
    }
}