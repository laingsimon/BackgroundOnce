using BackgroundOnce.EFCore.Plugin;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(BackgroundOnceEFCorePlugin))]

namespace BackgroundOnce.EFCore.Plugin
{
    public class BackgroundOnceEFCorePlugin : IRuntimePlugin
    {
        public void Initialize(
            RuntimePluginEvents runtimePluginEvents,
            RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEventsOnCustomizeGlobalDependencies;
        }

        private void RuntimePluginEventsOnCustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<IInMemorySnapshotHelper>(typeof(InMemorySnapshotHelper));
            e.ObjectContainer.RegisterTypeAs<IInMemorySnapshotTableFactory>(typeof(InMemorySnapshotTableFactory));
        }
    }
}