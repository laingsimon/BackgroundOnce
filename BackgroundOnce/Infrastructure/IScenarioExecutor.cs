using System.Threading.Tasks;

namespace BackgroundOnce.Infrastructure
{
    internal interface IScenarioExecutor
    {
        Task InvokeSubScenario(string scenarioName);
    }
}