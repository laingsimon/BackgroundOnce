using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Specflow.BackgroundOnce.EFCore.Extensions;

namespace Specflow.BackgroundOnce.EFCore
{
    internal class InMemorySnapshotTable : IInMemorySnapshotTable
    {
        private readonly IReadOnlyCollection<IInMemorySnapshotRow> _rows;

        public InMemorySnapshotTable(IReadOnlyCollection<IInMemorySnapshotRow> rows)
        {
            _rows = rows;
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
    }
}