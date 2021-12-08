using System.Linq;
using System.Reflection;
using BoDi;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Infrastructure;

namespace Specflow.BackgroundOnce.Extensions
{
    internal static class ScenarioContextExtensions
    {
        public static ScenarioContext CloneWithObjectContainer(
            this ScenarioContext scenarioContext,
            IObjectContainer objectContainer,
            ITestObjectResolver testObjectResolver = null)
        {
            var constructor = typeof(ScenarioContext).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            return (ScenarioContext)constructor.Invoke(new object[] { objectContainer, scenarioContext.ScenarioInfo, testObjectResolver });
        }
    }
}