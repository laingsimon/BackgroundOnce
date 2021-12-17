using System.Collections;

namespace BackgroundOnce.EFCore.InMemory
{
    public class InMemorySnapshotRow : IInMemorySnapshotRow
    {
        private readonly object[] _values;

        public InMemorySnapshotRow(object key, object[] values)
        {
            Key = key;
            _values = values;
        }

        public object Key { get; }

        public void AddTo(IDictionary rowDic)
        {
            rowDic.Add(Key, _values);
        }
    }
}