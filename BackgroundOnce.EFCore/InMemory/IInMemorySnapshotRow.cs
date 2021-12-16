using System.Collections;

namespace BackgroundOnce.EFCore.InMemory
{
    public interface IInMemorySnapshotRow
    {
        void AddTo(IDictionary rowDic);
    }
}