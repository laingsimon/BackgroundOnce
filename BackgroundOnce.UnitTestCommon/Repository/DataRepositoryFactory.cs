using BackgroundOnce.UnitTestCommon.Context;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public class DataRepositoryFactory
    {
        private readonly IDatabaseFactory _databaseFactory;

        public DataRepositoryFactory(DatabaseFactory databaseFactory)
        {
            _databaseFactory = databaseFactory;
        }

        public IDataRepository GetRepository(DataContext dataContext)
        {
            return new DataRepository(() => _databaseFactory.GetDatabase(dataContext.DatabaseType));
        }
    }
}