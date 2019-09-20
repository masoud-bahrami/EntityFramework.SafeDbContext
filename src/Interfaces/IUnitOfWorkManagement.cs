using System;
using System.Threading.Tasks;

namespace EntityFramework.SafeDbContext
{
    public interface IUnitOfWorkManagement
    {
        Task Start();
        Task Finish(Exception exception = null);
        Task StartAsync();
        Task FinishAsync(Exception exception = null);
    }

    public struct NullUnitOfWorkManagement : IUnitOfWorkManagement
    {
        public static IUnitOfWorkManagement New()
        {
            return new NullUnitOfWorkManagement();
        }
        public Task Start()
        {
            return Task.CompletedTask;
        }

        public Task Finish(Exception exception = null)
        {
            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            return Task.CompletedTask;
        }

        public Task FinishAsync(Exception exception = null)
        {
            return Task.CompletedTask;
        }
    }

    public class DefaultUnitOfWorkManagement : IUnitOfWorkManagement
    {
        private IAmbientDbContext _ambientDbContext;
        private readonly IAmbientDbContextFactory _ambientDbContextFactory;
        public DefaultUnitOfWorkManagement(IAmbientDbContextFactory ambientDbContextFactory)
        {
            _ambientDbContextFactory = ambientDbContextFactory;
        }
        public virtual Task Start()
        {
            _ambientDbContext = _ambientDbContextFactory.Create();
            return Task.CompletedTask;
        }

        public virtual Task Finish(Exception exception = null)
        {
            if (_ambientDbContext == null || !_ambientDbContext.CanRollBackOrCommit())
                return Task.CompletedTask;
            if (exception == null)
            {
                if (_ambientDbContext is AmbientDbContext context)
                {
                    context.ForceDispose();
                }
                else
                {
                     _ambientDbContext.SaveChanges();
                }
            }
            else
            {
                _ambientDbContext.RollBack();
            }

            return Task.CompletedTask;
        }

        public Task StartAsync()
        {
            _ambientDbContext = _ambientDbContextFactory.Create();
            return Task.CompletedTask;
        }

        public async Task FinishAsync(Exception exception = null)
        {
            if (_ambientDbContext == null || !_ambientDbContext.CanRollBackOrCommit())
                return ;

            if (exception == null)
            {
                if (_ambientDbContext is AmbientDbContext context)
                {
                    await context.ForceDisposeAsync();
                }
                else
                {
                    await _ambientDbContext.SaveChangesAsync();
                }
            }
            else
            {
                _ambientDbContext.RollBack();
            }
        }
    }
}