using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EntityFramework.SafeDbContext
{
    public class DbContextHolder : IDbContextHolder
    {
        private bool _isDisposed;
        private bool _committed;
        private readonly bool _isReadonly;
        private readonly IsolationLevel? _isolationLevel;
        private readonly Dictionary<Type, DbContext> _instantiatedDbContextCollection;
        private readonly Dictionary<DbContext, IDbContextTransaction> _instantiatedDbContextTransaction;
        private readonly IDbContextFactory _dbContextFactory;
        public DbContextHolder(bool isReadonly, IsolationLevel? isolationLevel, IDbContextFactory dbContextFactory = null)
        {
            _isReadonly = isReadonly;
            _isolationLevel = isolationLevel;
            _dbContextFactory = dbContextFactory;
            _isDisposed = false;
            _instantiatedDbContextCollection = new Dictionary<Type, DbContext>();
            _instantiatedDbContextTransaction = new Dictionary<DbContext, IDbContextTransaction>();
        }

        public TDbContext Get<TDbContext>()
            where TDbContext : DbContext
        {
            var specifiedDbContextType = typeof(TDbContext);
            if (_instantiatedDbContextCollection.TryGetValue(specifiedDbContextType, out var dbContext))
            {
                return dbContext as TDbContext;
            }

            var dbContextInstance = DbContextInstance<TDbContext>();


            if (_isReadonly)
            {
                dbContextInstance.ChangeTracker.AutoDetectChangesEnabled = false;
            }

            if (_isolationLevel.HasValue)
            {
                var transaction = dbContextInstance.Database.BeginTransaction(_isolationLevel.Value);
                _instantiatedDbContextTransaction.Add(dbContextInstance, transaction);
            }

            _instantiatedDbContextCollection.Add(specifiedDbContextType, dbContextInstance);

            return (TDbContext)dbContextInstance;
        }

        protected virtual TDbContext DbContextInstance<TDbContext>() where TDbContext : DbContext
        {
            return _dbContextFactory != null
                ? _dbContextFactory.Create<TDbContext>()
                : Activator.CreateInstance<TDbContext>();
        }

        public int Commit()
        {
            ExceptionDispatchInfo lastError = null;

            var c = 0;
            if (!_isDisposed && !_committed)
            {
                foreach (var dbContext in _instantiatedDbContextCollection.Values)
                {
                    try
                    {
                        if (!_isReadonly)
                        {
                            dbContext.NotifySubscribers();

                            c += dbContext.SaveChanges();
                        }

                        if (!_instantiatedDbContextTransaction.TryGetValue(dbContext, out var transaction))
                        {
                            continue;
                        }

                        transaction.Commit();
                        transaction.Dispose();
                    }
                    catch (Exception exception)
                    {
                        lastError = ExceptionDispatchInfo.Capture(exception);
                    }
                }
            }
            _committed = true;

            lastError?.Throw(); // Re-throw while maintaining the exception's original stack track

            return c;
        }

        public async Task<int> CommitAsync(CancellationToken? token = null)
        {
            ExceptionDispatchInfo lastError = null;

            int c = 0;
            if (!_isDisposed && !_committed)
            {
                if (token == null)
                {
                    token = CancellationToken.None;
                }

                foreach (var dbContext in _instantiatedDbContextCollection.Values)
                {
                    try
                    {
                        if (!_isReadonly)
                        {
                            dbContext.NotifySubscribers();

                            c += await dbContext.SaveChangesAsync(token.Value)
                                .ConfigureAwait(true);
                        }

                        if (_instantiatedDbContextTransaction.TryGetValue(dbContext, out var transaction))
                        {
                            transaction.Commit();
                            transaction.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        lastError = ExceptionDispatchInfo.Capture(exception);
                    }
                }
            }

            _committed = true;
            if (lastError != null)
            {
                lastError?.Throw(); // Re-throw while maintaining the exception's original stack track
            }
            return c;
        }

        public void RollBack()
        {
            ExceptionDispatchInfo lastError = null;

            if (!_isDisposed)
            {
                foreach (var context in _instantiatedDbContextCollection.Values)
                {
                    try
                    {
                        if (_instantiatedDbContextTransaction.TryGetValue(context, out var transaction))
                        {
                            transaction.Rollback();
                            transaction.Dispose();
                        }
                    }
                    catch (Exception exception)
                    {
                        lastError = ExceptionDispatchInfo.Capture(exception);
                    }
                }
            }
            lastError?.Throw(); // Re-throw while maintaining the exception's original stack track

            _committed = true;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (!_committed)
            {
                if (!_isReadonly)
                {
                    RollBack();
                }
                else
                {
                    Commit();
                }
            }

            foreach (var dbContext in _instantiatedDbContextCollection.Values)
            {
                try
                {
                    dbContext.Dispose();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
            ClearDbContextCollections();
            _isDisposed = true;
        }

        private void ClearDbContextCollections()
        {
            _instantiatedDbContextCollection
                .Values
                .ToList()
                .ForEach(c =>
                {
                    try
                    {
                        c.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            _instantiatedDbContextCollection.Clear();
            _instantiatedDbContextTransaction.Clear();
        }
    }
}