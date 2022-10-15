using System.Collections.Generic;
using System.Threading.Tasks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace BackgroundOnce.Infrastructure
{
    [Binding]
    internal class BackgroundOnceSteps
    {
        private const string GivenRegexAsync = @"I invoke the (.+) scenario only once";
        private const string GivenRegexSync = @"I synchronously invoke the (.+) scenario only once";
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

        public static bool MatchesStepAsync(string scenarioName, string stepText, StepDefinitionKeyword keyword)
        {
            var matchingText = GivenRegexAsync.Replace("(.+)", scenarioName);
            return matchingText == stepText && (keyword == StepDefinitionKeyword.Given || keyword == StepDefinitionKeyword.And);
        }

        public static bool MatchesStepSync(string scenarioName, string stepText, StepDefinitionKeyword keyword)
        {
            var matchingText = GivenRegexSync.Replace("(.+)", scenarioName);
            return matchingText == stepText && (keyword == StepDefinitionKeyword.Given || keyword == StepDefinitionKeyword.And);
        }

        [Given(GivenRegexSync)]
        public void GivenIInvokeTheScenarioWithNameSync(string scenarioName)
        {
            GivenIInvokeTheScenarioWithNameAsync(scenarioName).Wait();
        }

        [Given(GivenRegexAsync)]
        public async Task GivenIInvokeTheScenarioWithNameAsync(string scenarioName)
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
                await _scenarioExecutor.InvokeSubScenario(scenarioName);
            }
            finally
            {
                backgroundOnceExecutionPath.Pop();
            }

            if (_scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK && backgroundOnceExecutionPath.Count == 0)
            {
                await _snapshotManager.CreateSnapshots();

                if (scenarioName == _scenarioContext.ScenarioInfo.Title)
                {
                    /* special case for when the background is invoked directly.
                       in this case the background calls the scenario, then the scenario executes itself.
                       if the data isn't reset then the subsequent steps in the scenario could cause duplication of data
                     */
                    await _snapshotManager.ResetToInitial();
                }
            }
        }
    }
}
