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

        // some logic borrowed from James Newton-King, http://www.newtonsoft.com
        public static void SetValue(this MemberInfo member, object property, object value)
        {
            if (member.MemberType == MemberTypes.Property)
                ((PropertyInfo)member).SetValue(property, value, null);
            else if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(property, value);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static object GetValue(this MemberInfo member, object property)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(property, null);
            else if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(property);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static Type GetType(this MemberInfo member) => member.MemberType switch
        {
            MemberTypes.Field => ((FieldInfo)member).FieldType,
            MemberTypes.Property => ((PropertyInfo)member).PropertyType,
            MemberTypes.Event => ((EventInfo)member).EventHandlerType,
            _ => throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member"),
        };
    }
}
