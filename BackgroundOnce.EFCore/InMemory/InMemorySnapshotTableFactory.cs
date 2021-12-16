using System.Collections;
using System.Linq;
using BackgroundOnce.EFCore.Extensions;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace BackgroundOnce.EFCore.InMemory
{
    internal class InMemorySnapshotTableFactory : IInMemorySnapshotTableFactory
    {
#pragma warning disable EF1001
        public IInMemorySnapshotTable Create(IInMemoryTable table)
#pragma warning restore EF1001
        {
            var tableRows = table.NonPublicField<IDictionary>("_rows");

            var rows = tableRows.Keys
                .Cast<object>()
                .Select(key => new InMemorySnapshotRow(key, (object[])tableRows[key]))
                .ToArray();

            return new InMemorySnapshotTable(rows);
        }
    }
}