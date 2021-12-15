using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BackgroundOnce.UnitTestCommon.Data;
using BackgroundOnce.UnitTestCommon.Repository;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon
{
    public class InMemoryDatabase : IDatabase
    {
        private const string SnapshotKey = nameof(SnapshotKey);

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

        public ICollection<Person> People => Get<Person>();
        public ICollection<Address> Addresses => Get<Address>();
        public ICollection<Department> Departments => Get<Department>();
        public Task SaveChangesAsync()
        {
            return Task.CompletedTask;
        }

        public ICollection<T> Get<T>() where T : class
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

        public Task Remove<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var table = (InMemoryTable<T>)Get<T>();
            table.RemoveRecords(predicate.Compile());
            return Task.CompletedTask;
        }

        public Task CreateSnapshot(FeatureContext featureContext)
        {
            var snapshot = CloneAsReadOnly();
            featureContext[SnapshotKey] = snapshot;
            return Task.CompletedTask;
        }

        public Task RestoreSnapshot(FeatureContext featureContext)
        {
            if (!featureContext.TryGetValue<InMemoryDatabase>(SnapshotKey, out var snapshot))
            {
                throw new InvalidOperationException("No snapshots available");
            }

            ResetDataTo(snapshot);
            return Task.CompletedTask;
        }

        public bool SnapshotExists(FeatureContext featureContext)
        {
            return featureContext.ContainsKey(SnapshotKey);
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

        public class InMemoryTable<T> : IInMemoryTable, ICollection<T>
        {
            private readonly List<T> _records;

            public InMemoryTable()
                :this(new List<T>())
            {
            }

            private InMemoryTable(List<T> records, bool readOnly = false)
            {
                _records = records;
                IsReadOnly = readOnly;
            }

            public int Count => _records.Count;
            public bool IsReadOnly { get; }

            public void Add(T item)
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Unable to add record to a readonly table");
                }

                _records.Add(item);
            }

            public void Clear()
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Unable to clear a readonly table");
                }

                _records.Clear();
            }

            public bool Contains(T item)
            {
                return _records.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                _records.CopyTo(array, arrayIndex);
            }

            public bool Remove(T item)
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Unable to remove record from a readonly table");
                }

                return _records.Remove(item);
            }

            public void RemoveRecords(Func<T, bool> toRemove)
            {
                if (IsReadOnly)
                {
                    throw new InvalidOperationException("Unable to remove records from a readonly table");
                }

                _records.RemoveAll(r => toRemove(r));
            }

            public IInMemoryTable CloneAsReadOnly()
            {
                if (IsReadOnly)
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
                var readOnlySuffix = IsReadOnly
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