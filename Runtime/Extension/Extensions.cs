using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class Extensions
    {
        public static IReadOnlyDictionary<string, Type> Types
        {
            get
            {
                if (_types == null)
                    Initialize();

                return _types!;
            }
        }

        public static IReadOnlyDictionary<Type, string> Names
        {
            get
            {
                if (_names == null)
                    Initialize();

                return _names!;
            }
        }

        public static IReadOnlyCollection<Type> GenericTypes
        {
            get
            {
                if (_genericTypes == null)
                    Initialize();

                return _genericTypes!;
            }
        }

        private static Dictionary<string, Type>? _types;
        private static Dictionary<Type, string>? _names;
        private static HashSet<Type>? _genericTypes;

        public static IEnumerable<Type> GetAssignableFrom(Type assignableFrom)
        {
            foreach (var type in Types.Values)
            {
                if (assignableFrom.IsAssignableFrom(type))
                    yield return type;
            }
        }

#if UNITY_EDITOR
        static Extensions()
        {
            Initialize();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Initialize()
        {
            _types = new(StringComparer.OrdinalIgnoreCase);
            _names = new();
            _genericTypes = new();

            foreach (Type type in ReflectionUtility.GetTypesWithAttribute<CreateExtensionAttribute>(AppDomain.CurrentDomain))
            {
                if (type.GetInterface(nameof(IExtension)) == null)
                    throw new ExtensionsException($"Object with '{nameof(CreateExtensionAttribute)}' are required to be derived from '{nameof(IExtension)}'");

                string name = type.Name.TrimDecoration("Extension").ToTitleCase();
                _names[type] = name;
                _types[name] = type;

                // If it inherits IExtension<>.
                Type? genericType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IExtension<>));
                if (genericType != null)
                    _genericTypes.Add(genericType);
            }
        }
    }

    public class ExtensionsException : Exception
    {
        public ExtensionsException(string message) : base(message) { }
    }
}
