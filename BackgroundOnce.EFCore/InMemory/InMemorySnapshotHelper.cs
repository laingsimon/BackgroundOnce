using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundOnce.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace BackgroundOnce.EFCore.InMemory
{
    public class InMemorySnapshotHelper : ISnapshotHelper
    {
        private readonly IInMemorySnapshotTableFactory _inMemorySnapshotTableFactory;

        public InMemorySnapshotHelper(IInMemorySnapshotTableFactory inMemorySnapshotTableFactory)
        {
            _inMemorySnapshotTableFactory = inMemorySnapshotTableFactory;
        }

        public Task CreateSnapshot(DbContext dbContext, FeatureContext featureContext)
        {
            if (SnapshotExists(dbContext, featureContext))
            {
                throw new InvalidOperationException("Snapshot has already been created, it might be intentional to replace the snapshot, but it's not expected");
            }

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
            if (!SnapshotExists(dbContext, featureContext))
            {
                throw new InvalidOperationException("Snapshot hasn't been created, yet");
            }

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