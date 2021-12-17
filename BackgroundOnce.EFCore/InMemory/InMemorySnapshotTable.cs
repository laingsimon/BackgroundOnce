using System;
using System.Collections;
using System.Collections.Generic;
using BackgroundOnce.EFCore.Extensions;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace BackgroundOnce.EFCore.InMemory
{
    internal class InMemorySnapshotTable : IInMemorySnapshotTable
    {
        private readonly IReadOnlyCollection<IInMemorySnapshotRow> _rows;
        private readonly Func<IInMemoryTable> _tableFactory;

        public InMemorySnapshotTable(IReadOnlyCollection<IInMemorySnapshotRow> rows, Func<IInMemoryTable> tableFactory)
        {
            _rows = rows;
            _tableFactory = tableFactory;
        }

#pragma warning disable EF1001
        public void Replace(IInMemoryTable table)
#pragma warning restore EF1001
        {
            var tableRows = table.NonPublicField<IDictionary>("_rows");

            tableRows.Clear();

            foreach (var row in _rows)
            {
                row.AddTo(tableRows);
            }
        }

#pragma warning disable EF1001
        public IInMemoryTable GetTable()
        {
            var table = _tableFactory();
            var tableRows = table.NonPublicField<IDictionary>("_rows");

            foreach (var row in _rows)
            {
                row.AddTo(tableRows);
            }

            return table;
        }
#pragma warning restore EF1001
    }
}