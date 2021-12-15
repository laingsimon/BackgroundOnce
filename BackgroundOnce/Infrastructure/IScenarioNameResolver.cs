using System.Reflection;

namespace BackgroundOnce.Infrastructure
{
    internal interface IScenarioNameResolver
    {
        string GetScenarioNameForMethod(MethodInfo method);
    }
}