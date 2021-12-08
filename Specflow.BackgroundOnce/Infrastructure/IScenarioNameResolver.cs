using System.Reflection;

namespace Specflow.BackgroundOnce.Infrastructure
{
    internal interface IScenarioNameResolver
    {
        string GetScenarioNameForMethod(MethodInfo method);
    }
}