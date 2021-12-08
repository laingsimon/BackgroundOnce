using System.Reflection;
using TechTalk.SpecFlow.Tracing;

namespace Specflow.BackgroundOnce.Extensions
{
    internal static class TestTracerExtensions
    {
        public static ITraceListener GetTraceListener(this ITestTracer testTracer)
        {
            var testTracerType = testTracer.GetType();
            var traceListenerField = testTracerType.GetField("traceListener", BindingFlags.Instance | BindingFlags.NonPublic);
            return (ITraceListener)traceListenerField!.GetValue(testTracer);
        }
    }
}