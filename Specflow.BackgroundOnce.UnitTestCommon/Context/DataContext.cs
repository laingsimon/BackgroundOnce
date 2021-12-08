using System;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.UnitTestCommon.Context
{
    public class DataContext : ISnapshotData
    {
        private const string SnapshotKey = nameof(SnapshotKey);

        private readonly FeatureContext _featureContext;

        public DataContext(FeatureContext featureContext)
        {
            _featureContext = featureContext;
        }

        public InMemoryDatabase Database { get; } = new InMemoryDatabase();

        public Task CreateSnapshot()
        {
            var snapshot = Database.CloneAsReadOnly();
            _featureContext[SnapshotKey] = snapshot;
            return Task.CompletedTask;
        }

        public Task RestoreSnapshot()
        {
            if (!_featureContext.TryGetValue<InMemoryDatabase>(SnapshotKey, out var snapshot))
            {
                throw new InvalidOperationException("No snapshots available");
            }

            Database.ResetDataTo(snapshot);
            return Task.CompletedTask;
        }
    }
}