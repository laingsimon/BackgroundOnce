using System.Collections;

namespace BackgroundOnce.EFCore.InMemory
{
    public interface IInMemorySnapshotRow
    {
        object Key { get; }
        void AddTo(IDictionary rowDic);
    }
}