using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.Repository
{
    public interface IDatabase
    {
        ICollection<Person> People { get; }
        ICollection<Address> Addresses { get; }
        ICollection<Department> Departments { get; }
        Task<int> SaveChangesAsync(CancellationToken token = default);
        ICollection<T> Get<T>()
            where T : class;
        Task Remove<T>(Expression<Func<T, bool>> predicate)
            where T : class;
        Task CreateSnapshot(FeatureContext featureContext);
        Task RestoreSnapshot(FeatureContext featureContext);
    }
}