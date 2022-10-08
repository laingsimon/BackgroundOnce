using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TechTalk.SpecFlow;

namespace BackgroundOnce.Extensions
{
    internal static class TestRunnerManagerExtensions
    {
        private static readonly FieldInfo TestRunnerRegistryField = typeof(TestRunnerManager).GetField("_testRunnerRegistry", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo TestRunnerManagerRegistryField = typeof(TestRunnerManager).GetField("_testRunnerManagerRegistry", BindingFlags.Static | BindingFlags.NonPublic);

        public static ITestRunner GetTestRunnerWithoutCreating(this ITestRunnerManager manager, string testWorkerId)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var testRunnerRegistry = (ConcurrentDictionary<string, ITestRunner>)TestRunnerRegistryField!.GetValue(manager);
            if (testRunnerRegistry == null)
            {
                throw new InvalidOperationException("Unable to retrieve test runner registry");
            }

            return testRunnerRegistry.ContainsKey(testWorkerId)
                ? testRunnerRegistry[testWorkerId]
                : null;
        }

        public static void ReplaceTestRunner(this ITestRunnerManager manager, string testWorkerId, ITestRunner testRunner)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var testRunnerRegistry = (ConcurrentDictionary<string, ITestRunner>)TestRunnerRegistryField!.GetValue(manager);
            if (testRunnerRegistry == null)
            {
                throw new InvalidOperationException("Unable to retrieve test runner registry");
            }

            if (testRunner == null)
            {
                if (testRunnerRegistry.ContainsKey(testWorkerId))
                {
                    testRunnerRegistry.Remove(testWorkerId, out _);
                }

                return;
            }

            testRunnerRegistry[testWorkerId] = testRunner;
        }

        public static ITestRunnerManager GetTestRunnerManager(this ITestRunner testRunner)
        {
            if (testRunner == null)
            {
                throw new ArgumentNullException(nameof(testRunner));
            }

            var testRunnerManagerRegistry = (ConcurrentDictionary<Assembly, ITestRunnerManager>)TestRunnerManagerRegistryField!.GetValue(null);

            if (testRunnerManagerRegistry == null)
            {
                throw new InvalidOperationException("Unable to retrieve test runner manager registry");
            }

            var pluginAssembly = typeof(TestRunnerManagerExtensions).Assembly;
            var nonPluginAssemblies = testRunnerManagerRegistry
                .Where(pair => pair.Key != pluginAssembly)
                .Select(pair => pair.Value);

            return (from assemblyScopedTestRunner in nonPluginAssemblies
                let testRunnerRegistry = (ConcurrentDictionary<string, ITestRunner>)TestRunnerRegistryField.GetValue(assemblyScopedTestRunner)
                where testRunnerRegistry != null && testRunnerRegistry.Values.Contains(testRunner)
                select assemblyScopedTestRunner).FirstOrDefault();
        }
    }
}