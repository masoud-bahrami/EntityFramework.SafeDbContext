using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.SafeDbContext
{
    public class AmbientDbContext : IAmbientDbContext
    {
        private readonly DbContextHolder _dbContextHolder;
        public IDbContextHolder DbContextHolder => _dbContextHolder;
        private short _instantiatedCount;
        private bool _completed;
        private readonly bool _readOnly;
        private readonly bool _isOrphan;
        private bool _disposed;
        public AmbientDbContext(bool @readonly,
            IsolationLevel? isolationLevel,
            IDbContextFactory dbContextFactory = null)
            : this(@readonly, false, isolationLevel, dbContextFactory)
        {
        }

        public AmbientDbContext(bool @readonly,
            bool isOrphan,
            IsolationLevel? isolationLevel,
            IDbContextFactory dbContextFactory = null)
        {
            _disposed = false;
            _completed = false;
            _instantiatedCount = 1;
            _readOnly = @readonly;
            _isOrphan = isOrphan;
            _dbContextHolder = new DbContextHolder(_readOnly, isolationLevel, dbContextFactory);
        }

        internal void IncrementInstantiatedCount()
        {
            ++_instantiatedCount;
        }

        private void DecrementInstantiatedCount()
        {
            --_instantiatedCount;
        }
        private void ResetInstantiatedCount()
        {
            _instantiatedCount = 1;
        }
        public int SaveChanges()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DbContextScope");
            }

            if (_completed)
            {
                throw new InvalidOperationException(
                    "You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");
            }

            var c = 0;

            if (_instantiatedCount <= 1)
            {
                c = _dbContextHolder.Commit();
                _completed = true;
            }

            return c;
        }

        public async Task<int> SaveChangesAsync(CancellationToken? cancelToken = null)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DbContextScope");
            }

            if (_completed)
            {
                throw new InvalidOperationException(
                    "You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");
            }

            var c = 0;
            if (_instantiatedCount <= 1)
            {
                c = await _dbContextHolder.CommitAsync(cancelToken)
                    .ConfigureAwait(false);
                _completed = true;
            }

            return c;
        }

        public bool CanRollBackOrCommit()
        {
            return !_disposed && !_completed;
        }
        public void RollBack()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("DbContextScope");
            }

            if (_completed)
            {
                throw new InvalidOperationException(
                    "You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");
            }

            _dbContextHolder.RollBack();
        }

        public async void Dispose()
        {
            DecrementInstantiatedCount();
            if (_instantiatedCount == 0)
            {
                if (!_completed)
                {
                    if (_readOnly)
                    {
                        RollBack();
                    }
                    else
                    {
                        await SaveChangesAsync();
                    }
                }

                _disposed = true;
                if (!_isOrphan)
                {
                    CallContext<IAmbientDbContext>.DeleteDate(AmbientDbContextConstants.AmbientDbContextScopeKey);
                }
            }
            else
            {
                _disposed = false;
                GC.KeepAlive(this);
            }
        }

        internal void ForceDispose()
        {
            ResetInstantiatedCount();
            if (!_completed)
            {
                if (_readOnly)
                {
                    RollBack();
                }
                else
                {
                    SaveChanges();
                }
            }

            CallContext<IAmbientDbContext>.DeleteDate(AmbientDbContextConstants.AmbientDbContextScopeKey);
        }



        public async Task ForceDisposeAsync()
        {
            ResetInstantiatedCount();
            if (!_completed)
            {
                if (_readOnly)
                {
                    RollBack();
                }
                else
                {
                    await SaveChangesAsync();
                }
            }

            CallContext<IAmbientDbContext>.DeleteDate(AmbientDbContextConstants.AmbientDbContextScopeKey);
        }

        public bool Disposed()
        {
            return _disposed;
        }
    }
}