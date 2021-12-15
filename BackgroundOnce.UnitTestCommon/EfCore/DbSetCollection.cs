using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BackgroundOnce.UnitTestCommon.EfCore
{
    public class DbSetCollection<T> : ICollection<T>
        where T : class
    {
        private readonly DbSet<T> _set;

        public DbSetCollection(DbSet<T> set)
        {
            _set = set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _set.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _set.Add(item);
        }

        public void Clear()
        {
            _set.RemoveRange(_set);
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => _set.Count();

        public bool IsReadOnly => false;
    }
}