using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Specflow.BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.UnitTestCommon.Repository
{
    public interface IDatabase
    {
        ICollection<Person> People { get; }
        ICollection<Address> Addresses { get; }
        ICollection<Department> Departments { get; }
        Task SaveChangesAsync();
        ICollection<T> Get<T>()
            where T : class;
        Task Remove<T>(Expression<Func<T, bool>> predicate)
            where T : class;
        Task CreateSnapshot(FeatureContext featureContext);
        Task RestoreSnapshot(FeatureContext featureContext);
        bool SnapshotExists(FeatureContext featureContext);
    }
}