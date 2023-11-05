using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public static class ExtensionExtensions
    {
        private readonly static Dictionary<Type, Dictionary<string, Action<IExtension, IExtension>>> _cached = new();
        private readonly static ParameterExpression _instanceParameterExpression = Expression.Parameter(typeof(IExtension), "instance");
        private readonly static ParameterExpression _targetParameterExpression = Expression.Parameter(typeof(IExtension), "target");

        public static IEnumerable<IExtension> GetDerivedExtensions(this IExtension instance)
        {
            IExtension? currentExtension = instance.ExtensionReference;

            while (currentExtension != null)
            {
                yield return currentExtension;

                currentExtension = currentExtension.ExtensionReference;
            }
        }

        public static void CopyTo(this IExtension instance, IExtension? target)
        {
            if (target == null)
                return;

            foreach (string propertyName in instance.PropertiesModified.Keys)
                CopyTo(instance, target, propertyName);
        }
        public static void CopyTo(this IExtension instance, IExtension? target, string propertyName)
        {
            if (target == null)
                return;

            Type instanceType = instance.GetType();
            Type targetType = target.GetType();

            if (!targetType.IsAssignableFrom(instanceType))
                throw new ArgumentException($"target: {targetType} must be assignable from {instanceType}.", nameof(target));

            if (!_cached.TryGetValue(instanceType, out var properties))
                _cached[instanceType] = properties = new();

            if (!properties.TryGetValue(propertyName, out var action))
            {
                var fieldInfo = instanceType.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                Type fieldType = fieldInfo.FieldType;

                var instanceBoxedExpression = Expression.Convert(_instanceParameterExpression, instanceType);
                var targetBoxedExpression = Expression.Convert(_targetParameterExpression, targetType);

                var instanceFieldExpression = Expression.Field(instanceBoxedExpression, fieldInfo);
                var targetFieldExpression = Expression.Field(targetBoxedExpression, fieldInfo);

                if (fieldType.IsAssignableFrom(typeof(AnimationCurve))) // Uses 'CopyFrom' when assigning curves to prevent pointing to the same.
                {
                    var copyFromExpression = Expression.Call(targetFieldExpression, "CopyFrom", null, instanceFieldExpression);

                    action = Expression.Lambda<Action<IExtension, IExtension>>(
                        copyFromExpression, _instanceParameterExpression, _targetParameterExpression).Compile();
                }
                else
                {
                    var assignExpression = Expression.Assign(targetFieldExpression, instanceFieldExpression);

                    action = Expression.Lambda<Action<IExtension, IExtension>>(
                        assignExpression, _instanceParameterExpression, _targetParameterExpression).Compile();
                }

                properties[propertyName] = action;
            }

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(target.Target, $"Copy property: '{propertyName}' from '{instance.Target}' to '{target.Target}'");
#endif
            action(instance, target);
        }
    }
}
