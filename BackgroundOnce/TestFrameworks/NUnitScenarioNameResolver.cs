using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BackgroundOnce.Infrastructure;

namespace BackgroundOnce.TestFrameworks
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class NUnitScenarioNameResolver : IScenarioNameResolver
    {
        public string GetScenarioNameForMethod(MethodInfo method)
        {
            var attributes = method.GetCustomAttributes();
            var attr = attributes.SingleOrDefault(NunitDescriptionAttributeInterface.IsDescriptionAttribute);

            return attr == null
                ? null
                : NunitDescriptionAttributeInterface.GetDescription(attr);
        }

        private class NunitDescriptionAttributeInterface
        {
            private const string FullName = "NUnit.Framework.DescriptionAttribute";

            private static readonly Dictionary<Attribute, string> AttributeDescriptionCache = new Dictionary<Attribute, string>();

            private readonly Attribute _attribute;
            private readonly Type _attributeType;

            private NunitDescriptionAttributeInterface(Attribute attribute)
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

                var attributeInterface = new NunitDescriptionAttributeInterface(attribute);
                var description = attributeInterface.GetProperties<string>("Description");
                AttributeDescriptionCache.Add(attribute, description);
                return description;
            }

            public static bool IsDescriptionAttribute(Attribute attribute)
            {
                return ((Type)attribute.TypeId).FullName == FullName;
            }

            private T GetProperties<T>(string name)
            {
                var propertiesProperty = _attributeType.GetProperty("Properties");
                var properties = new PropertyBagInterface(propertiesProperty!.GetValue(_attribute));

                return properties.Get<T>(name);
            }
        }

        private class PropertyBagInterface
        {
            private readonly object _propertyBag;
            private readonly Type _propertyBagType;

            public PropertyBagInterface(object propertyBag)
            {
                _propertyBag = propertyBag;
                _propertyBagType = propertyBag.GetType();
            }

            public T Get<T>(string name)
            {
                var getMethod = _propertyBagType.GetMethod("Get");
                var result = getMethod!.Invoke(_propertyBag, new object[] { name });
                return (T)result;
            }
        }
    }
}