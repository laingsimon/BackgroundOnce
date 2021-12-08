using Specflow.BackgroundOnce.UnitTestCommon.Context;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.XUnit
{
    [Binding]
    public class DataSteps : UnitTestCommon.Steps.DataSteps
    {
        public DataSteps(DataContext dataContext, RequestContext requestContext)
            : base(dataContext, requestContext)
        {
        }
    }
}