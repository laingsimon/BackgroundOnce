using TechTalk.SpecFlow;

namespace BackgroundOnce.Infrastructure
{
    internal interface IFeatureFactory
    {
        IFeatureInstance CreateFeatureInstance(
            IScenarioNameResolver nameResolver,
            string featureTypeName,
            TestRunner subTestRunner);
    }
}