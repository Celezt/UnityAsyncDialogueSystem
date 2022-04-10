using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public static class ReflectionUtility
    {
        public static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly) where T : Attribute =>
            assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(T)));

        public static IEnumerable<Type> GetTypesWithAttribute<T>(AppDomain appDomain) where T : Attribute
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = appDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
                types.AddRange(GetTypesWithAttribute<T>(assembly));

            return types;
        }

        private static IEnumerable<Type> GetDerivedTypes<T>(Assembly assembly) =>
            assembly.GetTypes().Where(t => t != typeof(T) && typeof(T).IsAssignableFrom(t));

        public static IEnumerable<Type> GetDerivedTypes<T>(AppDomain appDomain)
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = appDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
                types.AddRange(GetDerivedTypes<T>(assembly));

            return types;
        }
    }
}
