using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
#if UNITY_EDITOR
    [InitializeOnLoad]
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

        private static Dictionary<string, Type>? _types;
        private static Dictionary<Type, string>? _names;

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

            foreach (Type type in ReflectionUtility.GetTypesWithAttribute<CreateExtensionAttribute>(AppDomain.CurrentDomain))
            {
                if (type.GetInterface(nameof(IExtension)) == null)
                    throw new ExtensionsException($"Object with '{nameof(CreateExtensionAttribute)}' are required to be derived from '{nameof(IExtension)}'");

                string name = ObjectNames.NicifyVariableName(type.Name.TrimDecoration("Extension"));
                _names[type] = name;
                _types[name] = type;
            }
        }
    }

    public class ExtensionsException : Exception
    {
        public ExtensionsException(string message) : base(message) { }
    }
}
