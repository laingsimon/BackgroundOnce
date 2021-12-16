using BackgroundOnce.UnitTestCommon.Context;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public interface IDatabaseFactory
    {
        IDatabase GetDatabase(DatabaseType databaseType);
    }
}