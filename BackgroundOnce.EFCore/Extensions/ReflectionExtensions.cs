using System;
using System.Reflection;

namespace BackgroundOnce.EFCore.Extensions
{
    internal static class ReflectionExtensions
    {
        public static TProperty NonPublicProperty<TProperty>(this object instance, string propertyName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var property = instance.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (property == null)
            {
                throw new InvalidOperationException($"Unable to find `{propertyName}` property on {instance.GetType().FullName}");
            }

            return (TProperty)property.GetValue(instance);
        }

        public static TProperty NonPublicField<TProperty>(this object instance, string fieldName)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (field == null)
            {
                throw new InvalidOperationException($"Unable to find `{fieldName}` field on {instance.GetType().FullName}");
            }

            return (TProperty)field.GetValue(instance);
        }
    }
}