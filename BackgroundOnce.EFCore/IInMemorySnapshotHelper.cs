using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;

namespace BackgroundOnce.EFCore
{
    public interface IInMemorySnapshotHelper
    {
        Task CreateSnapshot(DbContext dbContext, FeatureContext featureContext);
        Task RestoreSnapshot(DbContext dbContext, FeatureContext featureContext);
        bool SnapshotExists(DbContext dbContext, FeatureContext featureContext);
    }
}