using System.Linq;
using System.Threading.Tasks;
using Specflow.BackgroundOnce.Extensions;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.Infrastructure
{
    internal class SnapshotManager : ISnapshotManager
    {
        private const string BackgroundOnceSnapshotKey = nameof(BackgroundOnceSnapshotKey);

        private readonly ScenarioContext _scenarioContext;
        private readonly FeatureContext _featureContext;

        public SnapshotManager(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            _scenarioContext = scenarioContext;
            _featureContext = featureContext;
        }

        public async Task RestoreSnapshots()
        {
            var snapshotData = (ISnapshotData[])_featureContext[BackgroundOnceSnapshotKey];

            foreach (var backgroundDataObject in snapshotData)
            {
                await backgroundDataObject.RestoreSnapshot().ConfigureAwait(false);
            }
        }

        public async Task CreateSnapshots()
        {
            var scenarioContainer = _scenarioContext.ScenarioContainer;
            var snapshotData = scenarioContainer.GetObjectPool().OfType<ISnapshotData>().ToArray();

            foreach (var snapshotDataInstance in snapshotData)
            {
                await snapshotDataInstance.CreateSnapshot().ConfigureAwait(false);
            }

            _featureContext[BackgroundOnceSnapshotKey] = snapshotData;
        }

        public bool SnapshotsExist()
        {
            return _featureContext.ContainsKey(BackgroundOnceSnapshotKey);
        }
    }
}