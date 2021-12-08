using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Specflow.BackgroundOnce.Infrastructure;

namespace Specflow.BackgroundOnce.TestFrameworks
{
    public class XUnitScenarioNameResolver : IScenarioNameResolver
    {
        public string GetScenarioNameForMethod(MethodInfo method)
        {
            var customAttributes = method.CustomAttributes;
            var descriptions = customAttributes
                .Select(a => new TraitAttribute(a))
                .Where(a => a.IsXUnitTraitAttribute)
                .Where(trait => trait.TraitType == "Description")
                .Select(trait => trait.TraitValue)
                .Where(desc => !string.IsNullOrEmpty(desc))
                .ToArray();

            return descriptions.SingleOrDefault();
        }

        private class TraitAttribute
        {
            private const string TypeName = "Xunit.TraitAttribute";

            private readonly Type _attributeType;
            private readonly IList<CustomAttributeTypedArgument> _constructorArgs;

            public TraitAttribute(CustomAttributeData customAttributeData)
            {
                _attributeType = customAttributeData.AttributeType;
                _constructorArgs = customAttributeData.ConstructorArguments;
            }

            public bool IsXUnitTraitAttribute =>
                _attributeType.FullName == TypeName
                && _constructorArgs.Count == 2;

            public string TraitType => (string)_constructorArgs[0].Value;

            public string TraitValue => (string)_constructorArgs[1].Value;
        }
    }
}