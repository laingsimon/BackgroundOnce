using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace BackgroundOnce.EFCore.InMemory
{
    public interface IInMemorySnapshotTable
    {
#pragma warning disable EF1001
        void Replace(IInMemoryTable table);
#pragma warning restore EF1001
    }
}