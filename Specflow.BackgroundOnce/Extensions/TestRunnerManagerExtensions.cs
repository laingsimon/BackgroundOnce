using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.Extensions
{
    internal static class TestRunnerManagerExtensions
    {
        private static readonly FieldInfo TestRunnerRegistryField = typeof(TestRunnerManager).GetField("testRunnerRegistry", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TestRunnerManagerRegistryField = typeof(TestRunnerManager).GetField("testRunnerManagerRegistry", BindingFlags.Static | BindingFlags.NonPublic);

        public static ITestRunner GetTestRunnerWithoutCreating(this ITestRunnerManager manager, int threadId)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var testRunnerRegistry = (Dictionary<int, ITestRunner>)TestRunnerRegistryField!.GetValue(manager);
            if (testRunnerRegistry == null)
            {
                throw new InvalidOperationException("Unable to retrieve test runner registry");
            }

            return testRunnerRegistry.ContainsKey(threadId)
                ? testRunnerRegistry[threadId]
                : null;
        }

        public static void ReplaceTestRunner(this ITestRunnerManager manager, int threadId, ITestRunner testRunner)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var testRunnerRegistry = (Dictionary<int, ITestRunner>)TestRunnerRegistryField!.GetValue(manager);
            if (testRunnerRegistry == null)
            {
                throw new InvalidOperationException("Unable to retrieve test runner registry");
            }

            if (testRunner == null)
            {
                if (testRunnerRegistry.ContainsKey(threadId))
                {
                    testRunnerRegistry.Remove(threadId);
                }

                return;
            }

            testRunnerRegistry[threadId] = testRunner;
        }

        public static ITestRunnerManager GetTestRunnerManager(this ITestRunner testRunner)
        {
            if (testRunner == null)
            {
                throw new ArgumentNullException(nameof(testRunner));
            }

            var testRunnerManagerRegistry = (Dictionary<Assembly, ITestRunnerManager>)TestRunnerManagerRegistryField!.GetValue(null);

            if (testRunnerManagerRegistry == null)
            {
                throw new InvalidOperationException("Unable to retrieve test runner manager registry");
            }

            var pluginAssembly = typeof(TestRunnerManagerExtensions).Assembly;
            var nonPluginAssemblies = testRunnerManagerRegistry
                .Where(pair => pair.Key != pluginAssembly)
                .Select(pair => pair.Value);

            return (from assemblyScopedTestRunner in nonPluginAssemblies
                let testRunnerRegistry = (Dictionary<int, ITestRunner>)TestRunnerRegistryField.GetValue(assemblyScopedTestRunner)
                where testRunnerRegistry != null && testRunnerRegistry.ContainsValue(testRunner)
                select assemblyScopedTestRunner).FirstOrDefault();
        }
    }
}