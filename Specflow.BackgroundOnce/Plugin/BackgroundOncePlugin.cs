using System;
using System.Linq;
using System.Reflection;
using Specflow.BackgroundOnce.Infrastructure;
using Specflow.BackgroundOnce.Plugin;
using Specflow.BackgroundOnce.TestFrameworks;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Bindings.Reflection;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(BackgroundOncePlugin))]

namespace Specflow.BackgroundOnce.Plugin
{
    public class BackgroundOncePlugin : IRuntimePlugin
    {
        public void Initialize(
            RuntimePluginEvents runtimePluginEvents,
            RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEventsOnCustomizeGlobalDependencies;
            runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEventsOnCustomizeScenarioDependencies;
            runtimePluginEvents.CustomizeFeatureDependencies += RuntimePluginEventsOnCustomizeFeatureDependencies;
        }

        private static void RuntimePluginEventsOnCustomizeFeatureDependencies(object sender, CustomizeFeatureDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<ITestFrameworkResolver>(typeof(TestFrameworkResolver));
            var testFrameworkResolver = e.ObjectContainer.Resolve<ITestFrameworkResolver>();
            var testFramework = testFrameworkResolver.ResolveTestFramework();

            e.ObjectContainer.RegisterTypeAs<IScenarioNameResolver>(GetScenarioNameResolver(testFramework));
        }

        private static void RuntimePluginEventsOnCustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
        {
            var bindingFactory = e.ObjectContainer.Resolve<IBindingFactory>();
            var bindingRegistry = e.ObjectContainer.Resolve<IBindingRegistry>();

            RegisterStepBindings<BackgroundOnceSteps, GivenAttribute>(
                bindingFactory,
                bindingRegistry,
                StepDefinitionType.Given);

            RegisterStepBindings<BackgroundOnceSteps, WhenAttribute>(
                bindingFactory,
                bindingRegistry,
                StepDefinitionType.When);

            RegisterStepBindings<BackgroundOnceSteps, ThenAttribute>(
                bindingFactory,
                bindingRegistry,
                StepDefinitionType.Then);

        }

        private static void RuntimePluginEventsOnCustomizeScenarioDependencies(object sender, CustomizeScenarioDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<IScenarioExecutor>(typeof(ScenarioExecutor));
            e.ObjectContainer.RegisterTypeAs<ISnapshotManager>(typeof(SnapshotManager));
            e.ObjectContainer.RegisterTypeAs<IFeatureFactory>(typeof(FeatureFactory));
        }

        private static void RegisterStepBindings<T, TAttribute>(IBindingFactory bindingFactory, IBindingRegistry bindingRegistry, StepDefinitionType stepDefinitionType)
            where TAttribute: StepDefinitionBaseAttribute
        {
            var publicMethods = typeof(T)
                .GetMethods()
                .Where(m => m.GetCustomAttribute<TAttribute>() != null);

            foreach (var method in publicMethods)
            {
                var attribute = method.GetCustomAttribute<TAttribute>();

                var stepBinding = bindingFactory.CreateStepBinding(
                    stepDefinitionType,
                    attribute!.Regex,
                    new RuntimeBindingMethod(method),
                    null);
                bindingRegistry.RegisterStepDefinitionBinding(stepBinding);
            }
        }

        private static Type GetScenarioNameResolver(TestFramework testFramework)
        {
            switch (testFramework)
            {
                case TestFramework.Unknown:
                    throw new InvalidOperationException("BackgroundOnce hasn't detected a test framework");
                case TestFramework.NUnit:
                    return typeof(NUnitScenarioNameResolver);
                case TestFramework.XUnit:
                    return typeof(XUnitScenarioNameResolver);
                case TestFramework.MsTest:
                    return typeof(MsTestScenarioNameResolver);
            }

            throw new InvalidOperationException($"BackgroundOnce doesn't yet support the {testFramework} test framework");
        }
    }
}