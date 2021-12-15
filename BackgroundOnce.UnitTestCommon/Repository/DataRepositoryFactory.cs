using System;
using BackgroundOnce.UnitTestCommon.Context;
using BackgroundOnce.UnitTestCommon.EfCore;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public class DataRepositoryFactory
    {
        private const string DatabaseKey = nameof(DatabaseKey);

        private readonly DbContextFactory _dbContextFactory;
        private readonly FeatureContext _featureContext;

        public DataRepositoryFactory(DbContextFactory dbContextFactory, FeatureContext featureContext)
        {
            _dbContextFactory = dbContextFactory;
            _featureContext = featureContext;
        }

        public IDataRepository GetRepository(DataContext dataContext)
        {
            return new DataRepository(() => GetDatabase(dataContext.DatabaseType));
        }

        private IDatabase GetDatabase(DatabaseType databaseType)
        {
            if (!_featureContext.ContainsKey(DatabaseKey))
            {
                _featureContext[DatabaseKey] = ConstructDatabase(databaseType);
            }

            return (IDatabase) _featureContext[DatabaseKey];
        }

        private IDatabase ConstructDatabase(DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case DatabaseType.InMemoryDatabase:
                    return new InMemoryDatabase();
                case DatabaseType.EfCoreInMemory:
                    return _dbContextFactory.CreateInMemoryDbContext();
                default:
                    throw new NotSupportedException($"Unsupported database type: {databaseType}");
            }
        }
    }
}