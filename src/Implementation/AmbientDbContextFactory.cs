using System.Data;

namespace EntityFramework.SafeDbContext
{
    public class AmbientDbContextFactory : IAmbientDbContextFactory
    {
        private readonly IDbContextFactory _dbContextFactory;

        public AmbientDbContextFactory(IDbContextFactory dbContextFactory = null)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IAmbientDbContext Create()
        {
            return Create(_dbContextFactory);
        }
        public IAmbientDbContext Create(DbContextInstantiationStrategy joiningOption)
        {
            return Create(_dbContextFactory, joiningOption);
        }
        public IAmbientDbContext Create(IDbContextFactory dbContextFactory, DbContextInstantiationStrategy joiningOption = DbContextInstantiationStrategy.AppendIfOneAlreadyInstantiated, IsolationLevel? isolationLevel = null)
        {
            if (joiningOption == DbContextInstantiationStrategy.CreateNewInstanceAnyway)
            {
                return new AmbientDbContext(@readonly: false, isOrphan: true, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory);
            }

            var ambientDbContext = CallContext<IAmbientDbContext>.GetData(AmbientDbContextConstants.AmbientDbContextScopeKey);
            if (ambientDbContext != null)
            {
                ((AmbientDbContext)ambientDbContext).IncrementInstantiatedCount();
                return ambientDbContext;
            }
            ambientDbContext = new AmbientDbContext(false, isolationLevel, dbContextFactory);
            CallContext<IAmbientDbContext>.SetData(AmbientDbContextConstants.AmbientDbContextScopeKey, ambientDbContext);

            return ambientDbContext;
        }

        public IReadonlyAmbientDbContext CreateReadonly()
        {
            return CreateReadonly(_dbContextFactory);
        }

        public IReadonlyAmbientDbContext CreateReadonly(IDbContextFactory dbContextFactory, DbContextInstantiationStrategy joiningOption = DbContextInstantiationStrategy.AppendIfOneAlreadyInstantiated, IsolationLevel? isolationLevel = null)
        {
            if (joiningOption == DbContextInstantiationStrategy.CreateNewInstanceAnyway)
            {
                return new ReadonlyAmbientDbContext(isOrphan: true, dbContextFactory: dbContextFactory);
            }

            var ambientDbContext = CallContext<IReadonlyAmbientDbContext>.GetData(AmbientDbContextConstants.AmbientDbContextScopeKey);
            if (ambientDbContext != null)
            {
                ((ReadonlyAmbientDbContext)ambientDbContext).IncrementInstantiatedCount();
                return ambientDbContext;
            }

            ambientDbContext = new ReadonlyAmbientDbContext(isolationLevel, dbContextFactory);
            CallContext<IReadonlyAmbientDbContext>.SetData(AmbientDbContextConstants.AmbientDbContextScopeKey, ambientDbContext);

            return ambientDbContext;
        }
    }
}