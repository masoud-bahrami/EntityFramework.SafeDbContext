using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace EntityFramework.SafeDbContext
{
    public class InMemoryDbContextFactory : IDbContextFactory
    {
        private readonly string _dataBaseName;
        private DbContext _dbContext;
        public InMemoryDbContextFactory(string dataBaseName)
        {
            _dataBaseName =  dataBaseName != string.Empty ? dataBaseName : "inMemoryDb";

        }
        public TDbContext Create<TDbContext>()
            where TDbContext : DbContext
        {
            if (_dbContext != null)
            {
                return (TDbContext)DetachAllEntities(_dbContext);
            }

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<TDbContext>()
                .UseInMemoryDatabase(_dataBaseName)
                .UseInternalServiceProvider(serviceProvider);

            var builderOptions = builder.Options;

            _dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), builderOptions);

            return (TDbContext)_dbContext;
        }

        public DbContext DetachAllEntities(DbContext context)
        {
            var changedEntriesCopy = context.ChangeTracker.Entries()
                //.Where(e => e.State == EntityState.Added ||
                //            e.State == EntityState.Modified ||
                //            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
            {
                entry.State = EntityState.Detached;
            }

            return context;
        }
    }
}