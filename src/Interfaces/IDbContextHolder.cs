using System;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.SafeDbContext
{
    public interface IDbContextHolder : IDisposable
    {
        TDbContext Get<TDbContext>() where TDbContext : DbContext;
    }
}