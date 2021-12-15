using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BackgroundOnce.Infrastructure;

namespace BackgroundOnce.TestFrameworks
{
    public class MsTestScenarioNameResolver : IScenarioNameResolver
    {
        public string GetScenarioNameForMethod(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes();
            var attr = attributes.SingleOrDefault(MsTestDescriptionAttributeInterface.IsDescriptionAttribute);

            return attr == null
                ? null
                : MsTestDescriptionAttributeInterface.GetDescription(attr);
        }

        private class MsTestDescriptionAttributeInterface
        {
            private const string FullName = "Microsoft.VisualStudio.TestTools.UnitTesting.DescriptionAttribute";

            private static readonly Dictionary<Attribute, string> AttributeDescriptionCache = new Dictionary<Attribute, string>();

            private readonly Attribute _attribute;
            private readonly Type _attributeType;

            private MsTestDescriptionAttributeInterface(Attribute attribute)
            {
                _attribute = attribute;
                _attributeType = attribute.GetType();
            }

            public static string GetDescription(Attribute attribute)
            {
                if (AttributeDescriptionCache.ContainsKey(attribute))
                {
                    return AttributeDescriptionCache[attribute];
                }

                var attributeInterface = new MsTestDescriptionAttributeInterface(attribute);
                var description = attributeInterface.Description;
                AttributeDescriptionCache.Add(attribute, description);
                return description;
            }

            public static bool IsDescriptionAttribute(Attribute attribute)
            {
                return ((Type)attribute.TypeId).FullName == FullName;
            }

            private string Description
            {
                get
                {
                    var descriptionProperty = _attributeType.GetProperty("Description");
                    return (string)descriptionProperty!.GetValue(_attribute);
                }
            }
        }
    }
}