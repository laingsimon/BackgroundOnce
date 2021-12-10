using Specflow.BackgroundOnce.UnitTestCommon.Context;
using Specflow.BackgroundOnce.UnitTestCommon.Repository;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.XUnit
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