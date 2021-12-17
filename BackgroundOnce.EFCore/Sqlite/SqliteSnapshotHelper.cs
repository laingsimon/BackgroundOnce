using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackgroundOnce.EFCore.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using TechTalk.SpecFlow;

namespace BackgroundOnce.EFCore.Sqlite
{
    public class SqliteSnapshotHelper : ISnapshotHelper
    {
        public Task CreateSnapshot(DbContext dbContext, FeatureContext featureContext)
        {
            if (SnapshotExists(featureContext))
            {
                throw new InvalidOperationException("Snapshot has already been created, it might be intentional to replace the snapshot, but it's not expected");
            }

            var dbConnection = GetConnection(dbContext);
            var tableNames = GetTableNames(dbConnection);

            var dataSnapshot = tableNames.ToDictionary(
                tableName => tableName,
                tableName => GetTableSnapshot(tableName, dbConnection));

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

            var dbConnection = GetConnection(dbContext);
            var tableNames = GetTableNames(dbConnection);
            var allTableNames = tableNames.Union(snapshot.SnapshotData.Keys);

            async Task RestoreTableSnapshot(string tableName)
            {
                var snapshotData = snapshot.SnapshotData.ContainsKey(tableName)
                    ? snapshot.SnapshotData[tableName]
                    : null;

                if (snapshotData != null)
                {
                    await snapshotData.ReplaceData(dbConnection);
                }
                else
                {
                    await dbConnection.TruncateTable(tableName);
                }
            }

            await Task.WhenAll(allTableNames.Select(RestoreTableSnapshot));
            dbContext.ChangeTracker.AcceptAllChanges();
        }

        public async Task ResetToInitial(DbContext dbContext, FeatureContext featureContext)
        {
            var dbConnection = GetConnection(dbContext);
            var tableNames = GetTableNames(dbConnection);
            var truncateTasks = tableNames.Select(tableName => dbConnection.TruncateTable(tableName)).ToArray();

            await Task.WhenAll(truncateTasks);

            var databaseFacade = (IInfrastructure<IServiceProvider>)dbContext.Database;
#pragma warning disable EF1001
            var stateManager = (IStateManager)databaseFacade.Instance.GetService(typeof(IStateManager));
            stateManager!.Clear();
#pragma warning restore EF1001
        }

        private static SqliteTableSnapshot GetTableSnapshot(string tableName, SqliteConnection dbConnection)
        {
            string[] columnNames = null;

            var rows = dbConnection.Select(
                $"SELECT * FROM {tableName}",
                reader =>
                {
                    if (columnNames == null)
                    {
                        columnNames = Enumerable
                            .Range(0, reader.FieldCount)
                            .Select(reader.GetName)
                            .ToArray();
                    }

                    return columnNames
                        .Select((columnName, index) => new {columnName, index})
                        .ToDictionary(
                            a => a.columnName,
                            a => reader.GetValue(a.index));
                });

            return new SqliteTableSnapshot(tableName, rows);
        }

        private static IEnumerable<string> GetTableNames(SqliteConnection dbConnection)
        {
            return dbConnection.Select(
                "SELECT name from sqlite_schema WHERE type = 'table' and name NOT like 'sqlite_%' and name <> '__EFMigrationsHistory'",
                reader => reader.GetString(0));
        }

        private static SqliteConnection GetConnection(DbContext dbContext)
        {
            var databaseFacade = dbContext.Database;
            var dependencies = databaseFacade.NonPublicProperty<IRelationalDatabaseFacadeDependencies>("Dependencies");
            var relationalConnection = dependencies.RelationalConnection;
            return (SqliteConnection)relationalConnection.DbConnection;
        }

        private static bool SnapshotExists(FeatureContext featureContext)
        {
            return featureContext.ContainsKey(Snapshot.SnapshotKey);
        }

        private class Snapshot
        {
            public const string SnapshotKey = nameof(SnapshotKey);

            public Snapshot(Dictionary<string, SqliteTableSnapshot> snapshotData)
            {
                SnapshotData = snapshotData;
            }

            public Dictionary<string, SqliteTableSnapshot> SnapshotData { get; }
        }
    }
}