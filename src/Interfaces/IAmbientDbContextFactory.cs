using System.Data;

namespace EntityFramework.SafeDbContext
{
    public interface IAmbientDbContextFactory
    {
        IAmbientDbContext Create();
        IAmbientDbContext Create(DbContextInstantiationStrategy joiningOption);
        IAmbientDbContext Create(
            IDbContextFactory dbContextFactory, 
            DbContextInstantiationStrategy joiningOption = DbContextInstantiationStrategy.AppendIfOneAlreadyInstantiated, 
            IsolationLevel? isolationLevel = null);

        IReadonlyAmbientDbContext CreateReadonly();
        IReadonlyAmbientDbContext CreateReadonly(
            IDbContextFactory dbContextFactory ,
            DbContextInstantiationStrategy joiningOption = DbContextInstantiationStrategy.AppendIfOneAlreadyInstantiated,
            IsolationLevel? isolationLevel = null);
    }
}