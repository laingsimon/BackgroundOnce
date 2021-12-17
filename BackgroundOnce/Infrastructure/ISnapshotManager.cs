using System.Threading.Tasks;

namespace BackgroundOnce.Infrastructure
{
    public interface ISnapshotManager
    {
        Task RestoreSnapshots();

        Task CreateSnapshots();

        bool SnapshotsExist();

        Task ResetToInitial();
    }
}