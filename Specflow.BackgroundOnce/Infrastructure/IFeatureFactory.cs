using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.Infrastructure
{
    internal interface IFeatureFactory
    {
        IFeatureInstance CreateFeatureInstance(
            IScenarioNameResolver nameResolver,
            string featureTypeName,
            TestRunner subTestRunner);
    }
}