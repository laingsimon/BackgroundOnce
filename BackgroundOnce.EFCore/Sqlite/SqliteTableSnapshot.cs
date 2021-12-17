using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BackgroundOnce.EFCore.Sqlite
{
    public class SqliteTableSnapshot
    {
        private readonly string _tableName;
        private readonly IReadOnlyCollection<Dictionary<string, object>> _rows;

        public SqliteTableSnapshot(string tableName, IEnumerable<Dictionary<string, object>> rows)
        {
            _tableName = tableName;
            _rows = rows.ToArray();
        }

        public async Task ReplaceData(SqliteConnection dbConnection)
        {
            await dbConnection.TruncateTable(_tableName);

            foreach (var row in _rows)
            {
                await InsertRow(dbConnection, row);
            }
        }

        private async Task InsertRow(SqliteConnection dbConnection, Dictionary<string, object> row)
        {
            var columnNames = row.Keys;
            var columnValuesEscaped = columnNames.Select(columnName => GetEscapedValue(row[columnName]));

            var sql = $"INSERT INTO {_tableName} ({string.Join(", ", columnNames)}) VALUES ({string.Join(", ", columnValuesEscaped)})";
            await dbConnection.ExecuteNonQuery(sql);
        }

        private static string GetEscapedValue(object value)
        {
            return value switch
            {
                null => "null",
                string stringValue => $"'{stringValue}'",
                DateTime dateTimeValue => $"'{dateTimeValue:yyyy-MM-ddTHH:mm:ss.fff}'",
                _ => value.ToString()
            };
        }
    }
}