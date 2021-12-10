using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Specflow.BackgroundOnce.EFCore
{
    public interface IInMemorySnapshotTableFactory
    {
#pragma warning disable EF1001
        IInMemorySnapshotTable Create(IInMemoryTable table);
#pragma warning restore EF1001
    }
}