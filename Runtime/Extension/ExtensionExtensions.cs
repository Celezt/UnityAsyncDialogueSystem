using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public static class ExtensionExtensions
    {
        private readonly static Dictionary<Type, Dictionary<string, Action<IExtension, IExtension>>> _cached = new();
        private readonly static ParameterExpression _instanceParameterExpression = Expression.Parameter(typeof(IExtension), "instance");
        private readonly static ParameterExpression _targetParameterExpression = Expression.Parameter(typeof(IExtension), "target");

        public static void CopyTo(this IExtension instance, IExtension target)
        {
            Undo.RecordObject(target.Target, $"Copied all properties from '{instance.Target}' to '{target.Target}'");
            foreach (string propertyName in instance.PropertiesModified.Keys)
                Internal_CopyTo(instance, target, propertyName);
            target.Version++;
        }
        public static void CopyTo(this IExtension instance, IExtension target, string propertyName)
        {
            Undo.RecordObject(target.Target, $"Copied property: '{propertyName}' from '{instance.Target}' to '{target.Target}'");
            Internal_CopyTo(instance, target, propertyName);
            target.Version++;
        }

        internal static void Internal_CopyTo(IExtension instance, IExtension target, string propertyName)
        {
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

            action(instance, target);
        }
    }
}
