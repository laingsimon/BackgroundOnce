using BackgroundOnce.UnitTestCommon.Context;
using BackgroundOnce.UnitTestCommon.Repository;
using TechTalk.SpecFlow;

namespace BackgroundOnce.Nunit
{
    [Binding]
    public class DataSteps : UnitTestCommon.Steps.DataSteps
    {
        public DataSteps(DataContext dataContext, RequestContext requestContext, DataRepositoryFactory dataRepositoryFactory)
            : base(dataContext, requestContext, dataRepositoryFactory)
        {
        }
    }
}