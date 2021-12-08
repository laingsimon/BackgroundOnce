using System;

namespace Specflow.BackgroundOnce.Infrastructure
{
    public interface IFeatureInstance : IDisposable
    {
        void InvokeScenario(string name);
    }
}