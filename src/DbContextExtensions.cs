using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.SafeDbContext
{
    public static class DbContextExtensions
    {
        public static void NotifySubscribers(this DbContext dbContext)
        {
            try
            {
                //TODO resolve list of all IDbContextHandler from for example a service container
                List<IDbContextHandler> dbContextNotifiers = new List<IDbContextHandler>();
                
                var entityEntries = dbContext.ChangeTracker.Entries();
                if (!entityEntries.Any() || !dbContextNotifiers.Any())
                {
                    return;
                }

                foreach (var notifier in dbContextNotifiers)
                {
                    foreach (var entry in entityEntries)
                    {
                        if (entry.State == EntityState.Added)
                        {
                            notifier.OnAdd(entry);
                        }

                        if (entry.State == EntityState.Modified)
                        {
                            notifier.OnUpdate(entry);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

    }
}
