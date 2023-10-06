using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            foreach (string propertyName in instance.PropertiesModified.Keys)
                instance.CopyTo(target, propertyName);

        }
        public static void CopyTo(this IExtension instance, IExtension target, string propertyName)
        {
            Type instanceType = instance.GetType();
            Type targetType = target.GetType();

            if (!targetType.IsAssignableFrom(instanceType))
                throw new ArgumentException($"target: {targetType} must be assignable from {instanceType}.", nameof(target));

            if (!_cached.TryGetValue(instanceType, out var properties))
                _cached[instanceType] = properties = new();

            if (!properties.TryGetValue(propertyName, out var copyToAction))
            {
                var fieldInfo = instanceType.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                var instanceBoxedExpression = Expression.Convert(_instanceParameterExpression, instanceType);
                var targetBoxedExpression = Expression.Convert(_targetParameterExpression, targetType);

                var instanceFieldExpression = Expression.Field(instanceBoxedExpression, fieldInfo);
                var targetFieldExpression = Expression.Field(targetBoxedExpression, fieldInfo);
                var assignExpression = Expression.Assign(targetFieldExpression, instanceFieldExpression);

                copyToAction = Expression.Lambda<Action<IExtension, IExtension>>(
                    assignExpression, _instanceParameterExpression, _targetParameterExpression).Compile();

                properties[propertyName] = copyToAction;           
            }

            copyToAction(instance, target);
        }
    }
}
