using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Specflow.BackgroundOnce.UnitTestCommon
{
    public class InMemoryDatabase
    {
        private readonly IDictionary<Type, IInMemoryTable> _data;
        private readonly bool _readOnly;

        public InMemoryDatabase()
            :this(new Dictionary<Type, IInMemoryTable>())
        {
        }

        private InMemoryDatabase(IDictionary<Type, IInMemoryTable> data, bool readOnly = false)
        {
            _data = data;
            _readOnly = readOnly;
        }

        public InMemoryTable<T> GetTable<T>()
        {
            if (!_data.ContainsKey(typeof(T)))
            {
                if (_readOnly)
                {
                    return null;
                }

                _data[typeof(T)] = new InMemoryTable<T>();
            }

            return (InMemoryTable<T>)_data[typeof(T)];
        }

        public override string ToString()
        {
            var readOnlyPrefix = _readOnly
                ? "(readonly) "
                : "";

            return $"{readOnlyPrefix}{_data.Keys.Count} table/s:\r\n" + string.Join("\r\n", _data.Values);
        }

        public InMemoryDatabase CloneAsReadOnly()
        {
            if (_readOnly)
            {
                return this;
            }

            return new InMemoryDatabase(_data.ToDictionary(pair => pair.Key, pair => pair.Value.CloneAsReadOnly()), true);
        }

        public void ResetDataTo(InMemoryDatabase snapshot)
        {
            if (_readOnly)
            {
                throw new InvalidOperationException("Unable to modify a readonly database");
            }

            _data.Clear();
            foreach (var pair in snapshot._data)
            {
                _data.Add(pair.Key, pair.Value.CloneAsWritable());
            }
        }

        public interface IInMemoryTable
        {
            IInMemoryTable CloneAsReadOnly();

            IInMemoryTable CloneAsWritable();
        }

        public class InMemoryTable<T> : IInMemoryTable, IEnumerable<T>
        {
            private readonly List<T> _records;
            private readonly bool _readOnly;

            public InMemoryTable()
                :this(new List<T>())
            {
            }

            private InMemoryTable(List<T> records, bool readOnly = false)
            {
                _records = records;
                _readOnly = readOnly;
            }

            public void AddRecords(IEnumerable<T> toAdd)
            {
                if (_readOnly)
                {
                    throw new InvalidOperationException("Unable to add records to a readonly table");
                }

                _records.AddRange(toAdd);
            }

            public void RemoveRecords(Predicate<T> toRemove)
            {
                if (_readOnly)
                {
                    throw new InvalidOperationException("Unable to remove records from a readonly table");
                }

                _records.RemoveAll(toRemove);
            }

            public IInMemoryTable CloneAsReadOnly()
            {
                if (_readOnly)
                {
                    return this;
                }

                return Clone(true);
            }

            public IInMemoryTable CloneAsWritable()
            {
                return Clone(false);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _records.GetEnumerator();
            }

            public override string ToString()
            {
                var readOnlySuffix = _readOnly
                    ? " (readonly)"
                    : "";

                return $"{typeof(T).Name} Count: {_records.Count}{readOnlySuffix}";
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private IInMemoryTable Clone(bool readOnly)
            {
                return new InMemoryTable<T>(_records.ToList(), readOnly);
            }
        }
    }
}