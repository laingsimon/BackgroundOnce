using System;
using BackgroundOnce.UnitTestCommon.Context;
using BackgroundOnce.UnitTestCommon.EfCore;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private const string DatabaseKey = nameof(DatabaseKey);

        private readonly FeatureContext _featureContext;
        private readonly DbContextFactory _dbContextFactory;

        public DatabaseFactory(FeatureContext featureContext, DbContextFactory dbContextFactory)
        {
            _featureContext = featureContext;
            _dbContextFactory = dbContextFactory;
        }

        public IDatabase GetDatabase(DatabaseType databaseType)
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