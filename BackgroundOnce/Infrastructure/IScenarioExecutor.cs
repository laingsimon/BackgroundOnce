namespace BackgroundOnce.Infrastructure
{
    internal interface IScenarioExecutor
    {
        void InvokeSubScenario(string scenarioName);
    }
}