using BackgroundOnce.EFCore.InMemory;
using BackgroundOnce.EFCore.Sqlite;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public class DbContextFactory
    {
        private const string InMemoryRoot = nameof(InMemoryRoot);
        private const string SqliteConnection = nameof(SqliteConnection);

        private readonly FeatureContext _featureContext;
        private readonly InMemorySnapshotHelper _inMemorySnapshotHelper;
        private readonly SqliteSnapshotHelper _sqliteSnapshotHelper;

        public DbContextFactory(
            FeatureContext featureContext,
            InMemorySnapshotHelper inMemorySnapshotHelper,
            SqliteSnapshotHelper sqliteSnapshotHelper)
        {
            _featureContext = featureContext;
            _inMemorySnapshotHelper = inMemorySnapshotHelper;
            _sqliteSnapshotHelper = sqliteSnapshotHelper;
        }

        public InMemoryDbContext CreateInMemoryDbContext()
        {
            if (!_featureContext.ContainsKey(InMemoryRoot))
            {
                _featureContext[InMemoryRoot] = new InMemoryDatabaseRoot();
            }

            var root = (InMemoryDatabaseRoot)_featureContext[InMemoryRoot];
            return new InMemoryDbContext(root, _inMemorySnapshotHelper);
        }

        public SqliteDbContext CreateSqliteDbContext()
        {
            var initialiseRequired = false;
            if (!_featureContext.ContainsKey(SqliteConnection))
            {
                var newConnection = new SqliteConnection("Filename=:memory:");
                newConnection.Open();

                // register the instance so Dispose is called when the container disposes all other resources
                _featureContext.FeatureContainer.RegisterInstanceAs(newConnection, typeof(SqliteConnection));
                _featureContext[SqliteConnection] = newConnection;

                initialiseRequired = true;
            }

            var connection = (SqliteConnection)_featureContext[SqliteConnection];
            var context = new SqliteDbContext(connection, _sqliteSnapshotHelper);

            if (initialiseRequired)
            {
                context.Database.Migrate();
            }

            return context;
        }
    }
}