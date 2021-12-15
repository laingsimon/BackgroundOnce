using BackgroundOnce.EFCore;
using Microsoft.EntityFrameworkCore.Storage;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public class DbContextFactory
    {
        private const string InMemoryRoot = nameof(InMemoryRoot);
        private readonly FeatureContext _featureContext;
        private readonly InMemorySnapshotHelper _snapshotHelper;

        public DbContextFactory(FeatureContext featureContext, InMemorySnapshotHelper snapshotHelper)
        {
            _featureContext = featureContext;
            _snapshotHelper = snapshotHelper;
        }

        public InMemoryDbContext CreateInMemoryDbContext()
        {
            if (!_featureContext.ContainsKey(InMemoryRoot))
            {
                _featureContext[InMemoryRoot] = new InMemoryDatabaseRoot();
            }

            var root = (InMemoryDatabaseRoot)_featureContext[InMemoryRoot];
            return new InMemoryDbContext(root, _snapshotHelper);
        }
    }
}