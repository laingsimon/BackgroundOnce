using System;
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

            return new InMemorySnapshotTable(rows, GetTableFactory(table));
        }

#pragma warning disable EF1001
        private static Func<IInMemoryTable> GetTableFactory(IInMemoryTable table)
        {
            var constructor = table.GetType().GetConstructors().Single();
            var entityType = table.EntityType;
            var baseTable = table.BaseTable;
            var sensitiveLoggingEnabled = table.NonPublicField<bool>("_sensitiveLoggingEnabled");

            return () => (IInMemoryTable)constructor.Invoke(new object[] { entityType, baseTable, sensitiveLoggingEnabled});
        }
#pragma warning restore EF1001
    }
}