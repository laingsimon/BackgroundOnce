using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BoDi;

namespace BackgroundOnce.Extensions
{
    internal static class ObjectContainerExtensions
    {
        private static FieldInfo _objectPoolField;
        private static FieldInfo _resolvedKeysField;

        public static IEnumerable<object> GetObjectPool(this IObjectContainer objectContainer)
        {
            _objectPoolField ??= GetField("objectPool", objectContainer);
            var pool = (IDictionary)_objectPoolField.GetValue(objectContainer);

            if (pool == null)
            {
                yield break;
            }

            foreach (DictionaryEntry item in pool)
            {
                yield return item.Value;
            }
        }

        public static object ResolveAsNonDisposable(this IObjectContainer objectContainer, Type type)
        {
            _objectPoolField ??= GetField("objectPool", objectContainer);
            _resolvedKeysField ??= GetField("resolvedKeys", objectContainer);

            var pool = (IDictionary)_objectPoolField.GetValue(objectContainer);
            var resolvedKeys = (IList)_resolvedKeysField.GetValue(objectContainer);

            if (pool == null)
            {
                throw new InvalidOperationException("ObjectContainer pool is null");
            }

            if (resolvedKeys == null)
            {
                throw new InvalidOperationException("ObjectContainer resolvedKeys is null");
            }

            var preResolutionKeys = pool.Keys.Cast<object>().ToArray();
            var instance = objectContainer.Resolve(type);
            var postResolutionKeys = pool.Keys.Cast<object>().ToArray();

            var newResolutionKeys = postResolutionKeys.Except(preResolutionKeys).ToArray();

            foreach (var newResolutionKey in newResolutionKeys)
            {
                var registrationKey = new RegistrationKeyProxy(newResolutionKey);
                var newResolutionInstance = pool[newResolutionKey];

                pool.Remove(newResolutionKey);
                resolvedKeys.Remove(newResolutionKey);

                objectContainer.RegisterInstanceAs(newResolutionInstance, registrationKey.Type, registrationKey.Name, dispose: false);
            }

            return instance;
        }

        private static FieldInfo GetField(string name, IObjectContainer objectContainer)
        {
            return objectContainer.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic) ?? NotAValidContainer(objectContainer);
        }

        // ReSharper disable once EntityNameCapturedOnly.Local
        private static FieldInfo NotAValidContainer(IObjectContainer objectContainer)
        {
            throw new ArgumentException("Given instance is not a valid IObjectContainer", nameof(objectContainer));
        }

        private class RegistrationKeyProxy
        {
            private readonly object _registrationKey;

            public RegistrationKeyProxy(object registrationKey)
            {
                _registrationKey = registrationKey;
            }

            public string Name
            {
                get
                {
                    var nameField = _registrationKey.GetType().GetField("Name", BindingFlags.Public | BindingFlags.Instance);
                    return (string)nameField!.GetValue(_registrationKey);
                }
            }

            public Type Type
            {
                get
                {
                    var typeField = _registrationKey.GetType().GetField("Type", BindingFlags.Public | BindingFlags.Instance);
                    return (Type)typeField!.GetValue(_registrationKey);
                }
            }
        }
    }
}