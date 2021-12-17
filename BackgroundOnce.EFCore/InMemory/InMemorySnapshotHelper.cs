using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundOnce.EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
            if (SnapshotExists(featureContext))
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

        public async Task RestoreSnapshot(DbContext dbContext, FeatureContext featureContext)
        {
            if (!SnapshotExists(featureContext))
            {
                throw new InvalidOperationException("Snapshot hasn't been created, yet");
            }

            var snapshot = (Snapshot)featureContext[Snapshot.SnapshotKey];
            var data = dbContext.Database.GetData();

            Task RestoreTableSnapshot(object tableKey)
            {
                var tableSnapshot = snapshot.DataSnapshot.ContainsKey(tableKey)
                    ? snapshot.DataSnapshot[tableKey]
                    : null;

                if (tableSnapshot == null)
                {
                    data.Remove(tableKey);
                    return Task.CompletedTask;
                }

#pragma warning disable EF1001
                var tableData = data.ContainsKey(tableKey)
                    ? data[tableKey]
                    : null;

                if (tableData == null)
                {
                    var table = tableSnapshot.GetTable();
                    data.Add(tableKey, table);
                }

                tableSnapshot.Replace(tableData);
                return Task.CompletedTask;
#pragma warning restore EF1001
            }

            await Task.WhenAll(data.Keys.Select(RestoreTableSnapshot));
            dbContext.ChangeTracker.AcceptAllChanges();
        }

        public Task ResetToInitial(DbContext dbContext, FeatureContext featureContext)
        {
            var data = dbContext.Database.GetData();

            foreach (var pair in data.ToArray())
            {
                var tableKey = pair.Key;
                data.Remove(tableKey);
            }

            dbContext.ChangeTracker.AcceptAllChanges();

            var databaseFacade = (IInfrastructure<IServiceProvider>)dbContext.Database;
#pragma warning disable EF1001
            var stateManager = (IStateManager)databaseFacade.Instance.GetService(typeof(IStateManager));
            stateManager!.Clear();
#pragma warning restore EF1001

            return Task.CompletedTask;
        }

        private static bool SnapshotExists(FeatureContext featureContext)
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