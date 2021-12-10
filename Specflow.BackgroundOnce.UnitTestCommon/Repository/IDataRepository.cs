using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Specflow.BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.UnitTestCommon.Repository
{
    public interface IDataRepository
    {
        Task AddData(ReferenceData referenceData);
        Task UpdateAddress(string code, Address newAddress);
        Task<PersonDetails> GetPersonDetails(string name);
        int GetCount<T>()
            where T : class;
        Task Remove<T>(Expression<Func<T, bool>> predicate)
            where T : class;
        IEnumerable<T> Get<T>()
            where T : class;
        Task CreateSnapshot(FeatureContext featureContext);
        Task RestoreSnapshot(FeatureContext featureContext);
        bool SnapshotExists(FeatureContext featureContext);
    }
}