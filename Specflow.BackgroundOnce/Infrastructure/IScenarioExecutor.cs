namespace Specflow.BackgroundOnce.Infrastructure
{
    internal interface IScenarioExecutor
    {
        void InvokeSubScenario(string scenarioName);
    }
}