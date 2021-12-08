using System.Threading.Tasks;

namespace Specflow.BackgroundOnce.Infrastructure
{
    public interface ISnapshotManager
    {
        Task RestoreSnapshots();

        Task CreateSnapshots();

        bool SnapshotsExist();
    }
}