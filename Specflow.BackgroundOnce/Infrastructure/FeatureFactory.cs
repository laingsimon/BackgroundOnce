using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Specflow.BackgroundOnce.Extensions;
using TechTalk.SpecFlow;

namespace Specflow.BackgroundOnce.Infrastructure
{
    internal class FeatureFactory : IFeatureFactory
    {
        private readonly ScenarioContext _scenarioContext;

        public FeatureFactory(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        public IFeatureInstance CreateFeatureInstance(
            IScenarioNameResolver nameResolver,
            string featureTypeName,
            TestRunner subTestRunner)
        {
            var featureType = FindDeclaringType(featureTypeName);
            var instance = _scenarioContext.ScenarioContainer.ResolveAsNonDisposable(featureType);

            return new FeatureInstance(
                instance,
                GetScenarios(featureType, nameResolver),
                subTestRunner);
        }

        private static Type FindDeclaringType(string featureTypeName)
        {
            var frames = new StackTrace().GetFrames();
            var featureDeclaredType = frames
                .FirstOrDefault(f => f?.GetMethod()?.DeclaringType?.Name == featureTypeName)
                ?.GetMethod()?.DeclaringType;

            if (featureDeclaredType != null)
            {
                return featureDeclaredType;
            }

            var featureTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.Name.EndsWith(featureTypeName)))
                .ToArray();

            switch (featureTypes.Length)
            {
                case 0:
                    throw new InvalidOperationException($"Unable to find feature class for '{featureTypeName}'");
                case 1:
                    return featureTypes[0];
                default:
                    var typeNames = string.Join(Environment.NewLine, featureTypes.Select(t => $" - {t.FullName}"));
                    throw new InvalidOperationException($"Multiple types found for the given feature name: {typeNames}");
            }
        }

        private static IDictionary<string, MethodInfo> GetScenarios(Type featureType, IScenarioNameResolver scenarioNameResolver)
        {
            var scenarios =
                (from method in featureType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                let scenarioName = scenarioNameResolver.GetScenarioNameForMethod(method)
                where scenarioName != null
                select new {scenarioName, method })
                .ToDictionary(a => a.scenarioName, a => a.method);

            if (scenarios.Count == 0)
            {
                throw new InvalidOperationException($"Feature {featureType} has no scenarios identified");
            }

            return scenarios;
        }
    }
}