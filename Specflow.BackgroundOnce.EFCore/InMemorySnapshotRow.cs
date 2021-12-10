using System.Collections;

namespace Specflow.BackgroundOnce.EFCore
{
    public class InMemorySnapshotRow : IInMemorySnapshotRow
    {
        private readonly object _key;
        private readonly object[] _values;

        public InMemorySnapshotRow(object key, object[] values)
        {
            _key = key;
            _values = values;
        }

        public void AddTo(IDictionary rowDic)
        {
            rowDic.Add(_key, _values);
        }
    }
}