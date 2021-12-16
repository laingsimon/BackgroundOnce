using BackgroundOnce.EFCore.InMemory;
using Microsoft.EntityFrameworkCore.Storage;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public class DbContextFactory
    {
        private const string InMemoryRoot = nameof(InMemoryRoot);
        private readonly FeatureContext _featureContext;
        private readonly InMemorySnapshotHelper _inMemorySnapshotHelper;

        public DbContextFactory(FeatureContext featureContext, InMemorySnapshotHelper inMemorySnapshotHelper)
        {
            _featureContext = featureContext;
            _inMemorySnapshotHelper = inMemorySnapshotHelper;
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
    }
}