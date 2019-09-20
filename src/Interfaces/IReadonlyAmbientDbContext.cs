using System;

namespace EntityFramework.SafeDbContext
{
    public interface IReadonlyAmbientDbContext : IDisposable
    {
        IDbContextHolder DbContextHolder { get; }
    }
}