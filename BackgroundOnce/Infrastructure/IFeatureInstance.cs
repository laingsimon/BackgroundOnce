using System;

namespace BackgroundOnce.Infrastructure
{
    public interface IFeatureInstance : IDisposable
    {
        void InvokeScenario(string name);
    }
}