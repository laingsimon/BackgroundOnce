using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;

namespace Specflow.BackgroundOnce.EFCore.Extensions
{
#pragma warning disable EF1001
    internal static class InMemoryExtensions
    {
        public static Dictionary<object, IInMemoryTable> GetData(this DatabaseFacade facade)
        {
            var dependencies = facade.NonPublicProperty<DatabaseFacadeDependencies>("Dependencies");
            var databaseCreator = (InMemoryDatabaseCreator)dependencies.DatabaseCreator;
            var database = databaseCreator.NonPublicProperty<IInMemoryDatabase>("Database");
            var store = database.Store;
            return store.NonPublicField<Dictionary<object, IInMemoryTable>>("_tables");
        }
    }
#pragma warning restore EF1001
}