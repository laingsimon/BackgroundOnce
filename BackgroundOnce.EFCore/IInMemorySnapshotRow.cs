using System.Collections;

namespace BackgroundOnce.EFCore
{
    public interface IInMemorySnapshotRow
    {
        void AddTo(IDictionary rowDic);
    }
}