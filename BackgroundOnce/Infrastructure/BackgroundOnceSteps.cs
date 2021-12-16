using System.Collections.Generic;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace BackgroundOnce.Infrastructure
{
    [Binding]
    internal class BackgroundOnceSteps
    {
        private const string GivenRegex = @"I invoke the (.+) scenario only once";
        private const string BackgroundOnceExecutionPathKey = nameof(BackgroundOnceExecutionPathKey);

        private readonly IScenarioExecutor _scenarioExecutor;
        private readonly ScenarioContext _scenarioContext;
        private readonly ISnapshotManager _snapshotManager;

        public BackgroundOnceSteps(
            IScenarioExecutor scenarioExecutor,
            ScenarioContext scenarioContext,
            ISnapshotManager snapshotManager)
        {
            _scenarioExecutor = scenarioExecutor;
            _scenarioContext = scenarioContext;
            _snapshotManager = snapshotManager;
        }

        public static bool MatchesStep(string scenarioName, string stepText, StepDefinitionKeyword keyword)
        {
            var matchingText = GivenRegex.Replace("(.+)", scenarioName);
            return matchingText == stepText && (keyword == StepDefinitionKeyword.Given || keyword == StepDefinitionKeyword.And);
        }

        [Given(GivenRegex)]
        public async Task GivenIInvokeTheScenarioWithName(string scenarioName)
        {
            if (_snapshotManager.SnapshotsExist())
            {
                await _snapshotManager.RestoreSnapshots().ConfigureAwait(false);
                return;
            }

            if (!_scenarioContext.ContainsKey(BackgroundOnceExecutionPathKey))
            {
                _scenarioContext[BackgroundOnceExecutionPathKey] = new Stack<string>();
            }
            var backgroundOnceExecutionPath = (Stack<string>)_scenarioContext[BackgroundOnceExecutionPathKey];
            if (backgroundOnceExecutionPath.Contains(scenarioName))
            {
                // already executing this scenario, could be a recursive loop.
                return;
            }
            backgroundOnceExecutionPath.Push(scenarioName);

            try
            {
                _scenarioExecutor.InvokeSubScenario(scenarioName);

                if (_scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
                {
                    await _snapshotManager.CreateSnapshots();
                }
            }
            finally
            {
                backgroundOnceExecutionPath.Pop();
            }
        }
    }
}