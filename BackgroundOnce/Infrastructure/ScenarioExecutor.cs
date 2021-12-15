using System;
using System.Collections.Generic;
using System.Linq;
using BackgroundOnce.Extensions;
using BoDi;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Infrastructure;

namespace BackgroundOnce.Infrastructure
{
    internal class ScenarioExecutor : IScenarioExecutor
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly IFeatureFactory _featureFactory;
        private readonly IScenarioNameResolver _scenarioNameResolver;
        private readonly FeatureContext _featureContext;
        private readonly ITestRunner _testRunner;
        private readonly int _threadId;

        public ScenarioExecutor(
            ScenarioContext scenarioContext,
            IFeatureFactory featureFactory,
            IScenarioNameResolver scenarioNameResolver,
            FeatureContext featureContext,
            ITestRunner testRunner)
        {
            _scenarioContext = scenarioContext;
            _featureFactory = featureFactory;
            _scenarioNameResolver = scenarioNameResolver;
            _featureContext = featureContext;
            _testRunner = testRunner;
            _threadId = testRunner.ThreadId != 0
                ? testRunner.ThreadId
                : TestRunnerManager.GetTestRunner().ThreadId;
        }

        public void InvokeSubScenario(string scenarioName)
        {
            var recordingResult = RecordSteps(scenarioName);

            foreach (var step in recordingResult.Steps)
            {
                if (BackgroundOnceSteps.MatchesStep(scenarioName, step.Text, step.Keyword))
                {
                    // prevent a recursive loop of Scenario, invokes Background, invokes Scenario, invokes Background, ...
                    continue;
                }

                step.Invoke(_testRunner);
            }

            LiftObjectsCreatedInSubScenarioToFeatureContainer(recordingResult.ObjectContainer);
        }

        private void LiftObjectsCreatedInSubScenarioToFeatureContainer(IObjectContainer objectContainer)
        {
            var featureContextContainer = _featureContext.FeatureContainer;
            foreach (var subScenarioBackgroundSteps in objectContainer.GetObjectPool().OfType<ISnapshotData>())
            {
                featureContextContainer.RegisterInstanceAs(subScenarioBackgroundSteps, subScenarioBackgroundSteps.GetType());
            }
        }

        private RecordingResult RecordSteps(string scenarioName)
        {
            var scenarioContextWithSelectivelyRegisteringContainer = CreateScenarioContext(_scenarioContext);

            var recordingEngine = new RecordingTestExecutionEngine
            {
                FeatureContext = _featureContext,
                ScenarioContext = scenarioContextWithSelectivelyRegisteringContainer,
            };
            var recordingRunner = new TestRunner(recordingEngine);
            recordingRunner.InitializeTestRunner(_threadId);

            // replace the test runner
            var manager = _testRunner.GetTestRunnerManager();

            if (manager == null)
            {
                throw new InvalidOperationException($"Unable to find TestRunnerManager for TestRunner with thread id {_testRunner.ThreadId}");
            }

            var originalTestRunner = manager.GetTestRunnerWithoutCreating(_threadId);
            manager.ReplaceTestRunner(_threadId, recordingRunner);

            var subFeature = _featureFactory.CreateFeatureInstance(
                _scenarioNameResolver,
                _featureContext.FeatureInfo.Title + "Feature",
                recordingRunner);

            subFeature.InvokeScenario(scenarioName);

            subFeature.Dispose();

            // restore the test runner
            manager.ReplaceTestRunner(_threadId, originalTestRunner);

            return new RecordingResult
            {
                Steps = recordingEngine.Steps,
                ObjectContainer = _testRunner.ScenarioContext.ScenarioContainer /*scenarioContextWithSelectivelyRegisteringContainer.ScenarioContainer*/,
            };
        }

        private static ScenarioContext CreateScenarioContext(ScenarioContext outerContext)
        {
            var scenarioContext = outerContext.CloneWithObjectContainer(
                new InterceptingObjectContainer(outerContext.ScenarioContainer),
                new TestObjectResolver());

            return scenarioContext;
        }

        private class RecordingResult
        {
            public IReadOnlyCollection<RecordedStep> Steps { get; set; }

            public IObjectContainer ObjectContainer { get; set; }
        }

        private class InterceptingObjectContainer : IObjectContainer
        {
            private readonly IObjectContainer _objectContainer;

            public InterceptingObjectContainer(IObjectContainer objectContainer)
            {
                _objectContainer = objectContainer;
            }

            public void RegisterInstanceAs<TInterface>(TInterface instance, string name = null, bool dispose = false) where TInterface : class
            {
                if (_objectContainer.IsRegistered<TInterface>())
                {
                    return;
                }

                _objectContainer.RegisterInstanceAs(instance, name, dispose);
            }

            #region delegating members
            public void Dispose()
            {
                _objectContainer.Dispose();
            }

            public IStrategyRegistration RegisterTypeAs<TType, TInterface>(string name = null) where TType : class, TInterface
            {
                return _objectContainer.RegisterTypeAs<TType, TInterface>(name);
            }

            public void RegisterInstanceAs(object instance, Type interfaceType, string name = null, bool dispose = false)
            {
                _objectContainer.RegisterInstanceAs(instance, interfaceType, name, dispose);
            }

            public IStrategyRegistration RegisterFactoryAs<TInterface>(Func<IObjectContainer, TInterface> factoryDelegate, string name = null)
            {
                return _objectContainer.RegisterFactoryAs(factoryDelegate, name);
            }

            public T Resolve<T>()
            {
                return _objectContainer.Resolve<T>();
            }

            public T Resolve<T>(string name)
            {
                return _objectContainer.Resolve<T>(name);
            }

            public object Resolve(Type typeToResolve, string name = null)
            {
                return _objectContainer.Resolve(typeToResolve, name);
            }

            public IEnumerable<T> ResolveAll<T>() where T : class
            {
                return _objectContainer.ResolveAll<T>();
            }

            public bool IsRegistered<T>()
            {
                return _objectContainer.IsRegistered<T>();
            }

            public bool IsRegistered<T>(string name)
            {
                return _objectContainer.IsRegistered<T>(name);
            }

            public event Action<object> ObjectCreated
            {
                add => _objectContainer.ObjectCreated += value;
                remove => _objectContainer.ObjectCreated -= value;
            }
            #endregion
        }

        private class RecordingTestExecutionEngine : ITestExecutionEngine
        {
            public List<RecordedStep> Steps { get; } = new List<RecordedStep>();

            public void Step(StepDefinitionKeyword stepDefinitionKeyword, string keyword, string text, string multilineTextArg, Table tableArg)
            {
                switch (stepDefinitionKeyword)
                {
                    case StepDefinitionKeyword.And:
                        Steps.Add(new RecordedStep(runner => runner.And(text, multilineTextArg, tableArg, keyword), stepDefinitionKeyword, text));
                        break;
                    case StepDefinitionKeyword.But:
                        Steps.Add(new RecordedStep(runner => runner.But(text, multilineTextArg, tableArg, keyword), stepDefinitionKeyword, text));
                        break;
                    case StepDefinitionKeyword.Given:
                        Steps.Add(new RecordedStep(runner => runner.Given(text, multilineTextArg, tableArg, keyword), stepDefinitionKeyword, text));
                        break;
                    case StepDefinitionKeyword.When:
                        Steps.Add(new RecordedStep(runner => runner.When(text, multilineTextArg, tableArg, keyword), stepDefinitionKeyword, text));
                        break;
                    case StepDefinitionKeyword.Then:
                        Steps.Add(new RecordedStep(runner => runner.Then(text, multilineTextArg, tableArg, keyword), stepDefinitionKeyword, text));
                        break;
                    default:
                        throw new NotSupportedException($"Gherkin keyword {stepDefinitionKeyword} isn't supported");
                }
            }

            #region empty members
            public void OnTestRunStart()
            {
            }

            public void OnTestRunEnd()
            {
            }

            public void OnFeatureStart(FeatureInfo featureInfo)
            {
            }

            public void OnFeatureEnd()
            {
            }

            public void OnScenarioInitialize(ScenarioInfo scenarioInfo)
            {
            }

            public void OnScenarioStart()
            {
            }

            public void OnAfterLastStep()
            {
            }

            public void OnScenarioEnd()
            {
            }

            public void OnScenarioSkipped()
            {
            }

            public void Pending()
            {
            }

            public FeatureContext FeatureContext { get; set; }
            public ScenarioContext ScenarioContext { get; set; }
            #endregion
        }

        private class RecordedStep
        {
            private readonly Action<ITestRunner> _action;

            public RecordedStep(Action<ITestRunner> action, StepDefinitionKeyword keyword, string text)
            {
                Keyword = keyword;
                Text = text;
                _action = action;
            }

            public StepDefinitionKeyword Keyword { get; }

            public string Text { get; }

            public void Invoke(ITestRunner testRunner)
            {
                _action(testRunner);
            }
        }
    }
}