using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.SafeDbContext
{
    /// <summary>
    /// SaveChanges and SaveChangesAsync methods in <see cref="RestrictedDbContext"/> are hidden.
    /// </summary>
    public class RestrictedDbContext : DbContext
    {
        private readonly Assembly _assembly;
        public RestrictedDbContext(DbContextOptions options, Assembly assembly = null)
            : base(options)
        {
            _assembly = assembly;
        }

       
        protected override  async void OnModelCreating(ModelBuilder builder)
        {
            builder.RegisterDbSetsConfig(_assembly);
            builder.RegisterDbSetsAsync(_assembly).Wait();
           
            try
            {
                base.OnModelCreating(builder);
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }
        }

        public DbSet<T> GetDbSet<T>() where T : class
        {
            return Set<T>();
        }

        [Obsolete("SaveChangesAsync is deprecated. No need to use this method. Do not do anything ", true)]
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }
        [Obsolete("SaveChangesAsync is deprecated. No need to use this method. Do not do anything ", true)]
        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            return result;
        }
        [Obsolete("SaveChangesAsync is deprecated. No need to use this method. Do not do anything ", true)]
        public override int SaveChanges()
        {
            var result = base.SaveChanges();
            return result;
        }
        [Obsolete("SaveChangesAsync is deprecated. No need to use this method. Do not do anything ", true)]
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            return result;
        }
    }
}