using System.Threading.Tasks;
using Specflow.BackgroundOnce.UnitTestCommon.Repository;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.UnitTestCommon.Context
{
    public class DataContext : ISnapshotData
    {
        private readonly FeatureContext _featureContext;
        private readonly DataRepositoryFactory _repositoryFactory;

        public DataContext(FeatureContext featureContext, DataRepositoryFactory repositoryFactory)
        {
            _featureContext = featureContext;
            _repositoryFactory = repositoryFactory;
        }

        public DatabaseType DatabaseType { get; set; }

        public async Task CreateSnapshot()
        {
            await _repositoryFactory.GetRepository(this).CreateSnapshot(_featureContext);
        }

        public async Task RestoreSnapshot()
        {
            await _repositoryFactory.GetRepository(this).RestoreSnapshot(_featureContext);
        }
    }
}