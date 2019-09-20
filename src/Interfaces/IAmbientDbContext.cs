using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.SafeDbContext
{
    public interface IAmbientDbContext : IDisposable
    {
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken? cancelToken = null);
        void RollBack();
        bool CanRollBackOrCommit();
        IDbContextHolder DbContextHolder { get; }
    }
}