using System;
using System.Data;

namespace EntityFramework.SafeDbContext
{
    public class ReadonlyAmbientDbContext : IReadonlyAmbientDbContext
    {
        private readonly AmbientDbContext _internalScope;

        public IDbContextHolder DbContextHolder => _internalScope.DbContextHolder;

        public ReadonlyAmbientDbContext(IsolationLevel? isolationLevel = null,
            IDbContextFactory dbContextFactory = null)
            : this(false, isolationLevel, dbContextFactory)
        {
        }
        public ReadonlyAmbientDbContext(bool isOrphan, IsolationLevel? isolationLevel = null, IDbContextFactory dbContextFactory = null)
        {
            _internalScope = new AmbientDbContext(@readonly: true, isOrphan: isOrphan, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory);
        }

        internal void IncrementInstantiatedCount()
        {
            _internalScope.IncrementInstantiatedCount();
        }
        public void Dispose()
        {
            _internalScope.Dispose();
            if (!_internalScope.Disposed())
                GC.KeepAlive(this);
        }
    }
}