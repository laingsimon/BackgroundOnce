using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace BackgroundOnce.EFCore.Sqlite
{
    public static class SqliteExtensions
    {
        public static IEnumerable<T> Select<T>(this SqliteConnection connection, string sql, Func<IDataReader, T> select)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    yield return select(reader);
                }
            }
        }

        public static async Task ExecuteNonQuery(this SqliteConnection connection, string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        public static async Task TruncateTable(this SqliteConnection dbConnection, string tableName)
        {
            await dbConnection.ExecuteNonQuery($"DELETE FROM {tableName}");
        }
    }
}