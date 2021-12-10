using System.Collections;

namespace Specflow.BackgroundOnce.EFCore
{
    public interface IInMemorySnapshotRow
    {
        void AddTo(IDictionary rowDic);
    }
}