using System;
using System.Collections.Generic;
using System.Reflection;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.Infrastructure
{
    internal class FeatureInstance : IFeatureInstance
    {
        private readonly object _featureInstance;
        private readonly IDictionary<string, MethodInfo> _scenarios;
        private readonly TestRunner _subTestRunner;
        private readonly FieldInfo _testRunnerField;
        private readonly TestRunner _previousTestRunner;

        public FeatureInstance(object featureInstance, IDictionary<string, MethodInfo> scenarios, TestRunner subTestRunner)
        {
            _featureInstance = featureInstance;
            _scenarios = scenarios;
            _subTestRunner = subTestRunner;

            var featureType = featureInstance.GetType();
            _testRunnerField = featureType.GetField("testRunner", BindingFlags.Instance | BindingFlags.NonPublic)
                             ?? featureType.GetField("testRunner", BindingFlags.Static | BindingFlags.NonPublic)
                             ?? NotAFeature(featureInstance);

            _previousTestRunner = (TestRunner)_testRunnerField.GetValue(_featureInstance);
            _testRunnerField.SetValue(_featureInstance, subTestRunner);
        }

        public void InvokeScenario(string name)
        {
            if (!_scenarios.ContainsKey(name))
            {
                throw new ArgumentOutOfRangeException(nameof(name), $"No scenario exists with name '{name}'");
            }

            var scenario = _scenarios[name];
            scenario.Invoke(_featureInstance, Array.Empty<object>());
        }

        public void Dispose()
        {
            try
            {
                _testRunnerField.SetValue(_featureInstance, _subTestRunner);
                if (_featureInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                else if (_featureInstance is IAsyncDisposable asyncDisposable)
                {
                    asyncDisposable.DisposeAsync().GetAwaiter().GetResult();
                }
            }
            finally
            {
                _testRunnerField.SetValue(_featureInstance, _previousTestRunner);
            }
        }

        // ReSharper disable once EntityNameCapturedOnly.Local
        private static FieldInfo NotAFeature(object featureInstance)
        {
            throw new ArgumentException("Given instance is not a Specflow feature", nameof(featureInstance));
        }
    }
}