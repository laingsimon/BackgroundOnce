using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Specflow.BackgroundOnce.EFCore.Extensions;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.EFCore
{
    public class InMemorySnapshotHelper : IInMemorySnapshotHelper
    {
        private readonly IInMemorySnapshotTableFactory _inMemorySnapshotTableFactory;

        public InMemorySnapshotHelper(IInMemorySnapshotTableFactory inMemorySnapshotTableFactory)
        {
            _inMemorySnapshotTableFactory = inMemorySnapshotTableFactory;
        }

        public Task CreateSnapshot(DbContext dbContext, FeatureContext featureContext)
        {
            var data = dbContext.Database.GetData();
            var dataSnapshot = data.ToDictionary(
                pair => pair.Key,
                pair => _inMemorySnapshotTableFactory.Create(pair.Value));

            var snapshot = new Snapshot(dataSnapshot);
            featureContext[Snapshot.SnapshotKey] = snapshot;
            return Task.CompletedTask;
        }

        public Task RestoreSnapshot(DbContext dbContext, FeatureContext featureContext)
        {
            var snapshot = (Snapshot)featureContext[Snapshot.SnapshotKey];

            var data = dbContext.Database.GetData();

            foreach (var pair in data.ToArray())
            {
                var tableKey = pair.Key;
#pragma warning disable EF1001
                var tableData = pair.Value;
#pragma warning restore EF1001

                var tableSnapshot = snapshot.DataSnapshot.ContainsKey(tableKey)
                    ? snapshot.DataSnapshot[tableKey]
                    : null;

                if (tableSnapshot == null)
                {
                    data.Remove(tableKey);
                    continue;
                }

                tableSnapshot.Replace(tableData);
            }

            dbContext.ChangeTracker.AcceptAllChanges();
            return Task.CompletedTask;
        }

        public bool SnapshotExists(DbContext dbContext, FeatureContext featureContext)
        {
            return featureContext.ContainsKey(Snapshot.SnapshotKey);
        }

        private class Snapshot
        {
            public const string SnapshotKey = nameof(SnapshotKey);

            public Snapshot(Dictionary<object, IInMemorySnapshotTable> dataSnapshot)
            {
                DataSnapshot = dataSnapshot;
            }

            public Dictionary<object, IInMemorySnapshotTable> DataSnapshot { get; }
        }
    }
}