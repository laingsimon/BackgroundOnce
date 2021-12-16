using System.Threading.Tasks;
using BackgroundOnce.UnitTestCommon.Repository;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.Context
{
    public class DataContext : ISnapshotData
    {
        private readonly FeatureContext _featureContext;
        private readonly DatabaseFactory _databaseFactory;

        public DataContext(FeatureContext featureContext, DatabaseFactory databaseFactory)
        {
            _featureContext = featureContext;
            _databaseFactory = databaseFactory;
        }

        public DatabaseType DatabaseType { get; set; }

        public async Task CreateSnapshot()
        {
            await _databaseFactory.GetDatabase(DatabaseType).CreateSnapshot(_featureContext);
        }

        public async Task RestoreSnapshot()
        {
            await _databaseFactory.GetDatabase(DatabaseType).RestoreSnapshot(_featureContext);
        }
    }
}