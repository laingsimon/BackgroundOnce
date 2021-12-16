using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BackgroundOnce.UnitTestCommon.Data;
using TechTalk.SpecFlow;

namespace BackgroundOnce.UnitTestCommon.Repository
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
    }
}