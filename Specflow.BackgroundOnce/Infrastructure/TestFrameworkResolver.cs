using Specflow.BackgroundOnce.Extensions;
using TechTalk.SpecFlow.Tracing;

namespace Specflow.BackgroundOnce.Infrastructure
{
    internal class TestFrameworkResolver : ITestFrameworkResolver
    {
        private readonly ITestTracer _testTracer;

        public TestFrameworkResolver(ITestTracer testTracer)
        {
            _testTracer = testTracer;
        }

        public TestFramework ResolveTestFramework()
        {
            var traceListener = _testTracer.GetTraceListener();
            var traceListenerType = traceListener.GetType();

            switch (traceListenerType.Name)
            {
                case "NUnitTraceListener":
                    return TestFramework.NUnit;
                case "XUnitTraceListener":
                    return TestFramework.XUnit;
                case "MSTestTraceListener":
                    return TestFramework.MsTest;
                default:
                    return TestFramework.Unknown;
            }
        }
    }
}