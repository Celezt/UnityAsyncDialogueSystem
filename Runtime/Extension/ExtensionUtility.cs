using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public static class ExtensionUtility
    {
        public static void AddExtensions(UnityEngine.Object target, IExtensionCollection? toAddCollection, IList<IExtension> extensions)
        {
            if (toAddCollection == null)
                return;

            UnityEngine.Object? reference = toAddCollection as UnityEngine.Object;
            if (reference)
            {
                if (reference == target)
                {
                    Debug.LogWarning("Cannot reference itself.");
                    return;
                }

                if (toAddCollection.Extensions == null || toAddCollection.Count() == 0)
                {
                    Debug.LogWarning($"The extension object: {toAddCollection} does not contain any extensions. Try add an extension to it.");
                    return;
                }
            }

            foreach (var toAdd in toAddCollection)
                AddExtension(target, toAdd, extensions, reference);
        }

        public static void AddExtension(UnityEngine.Object target, IExtension? toAdd, IList<IExtension> extensions, UnityEngine.Object? reference = null)
        {
            if (toAdd == null)
                return;

            Type type = toAdd.GetType();
            if (!extensions.Any(x => x.GetType() == type))  // Duplicates are not allowed.
            {
                toAdd.Reference = reference;

                if (!HasSelfReference(target, toAdd.Reference))
                    extensions.Add(toAdd);
            }
            else
                Debug.LogWarning("Duplicates are not allowed.");
        }

        public static void RemoveExtension(Type type, IList<IExtension> extensions)
        {
            int index = extensions.FindIndex(x => type.IsAssignableFrom(x.GetType()));

            if (index < 0)
                return;

            extensions.RemoveAt(index);
        }

        public static void MoveUpExtension(Type type, IList<IExtension> extensions)
        {
            int index = extensions.FindIndex(x => type.IsAssignableFrom(x.GetType()));
            extensions.Move(index, index - 1);  // Index 0 is at top.
        }

        public static void MoveDownExtension(Type type, IList<IExtension> extensions)
        {
            int index = extensions.FindIndex(x => type.IsAssignableFrom(x.GetType()));
            extensions.Move(index, index + 1);  // Index 0 is at top.
        }

        public static bool HasSelfReference(UnityEngine.Object owner, UnityEngine.Object? reference)
        {
            if (reference == null)
                return false;

            if (owner == reference)
            {
                Debug.LogWarning("Reference hierarchy is not allowed to contain itself.");
                return true;
            }

            if (reference is IExtensionCollection collectionReference)
            {
                foreach (var childReference in collectionReference.Where(x => x.Reference != null).Select(x => x.Reference))
                {
                    if (HasSelfReference(owner, childReference))
                        return true;
                }
            }

            return false;
        }

        public static bool TryGetExtension(UnityEngine.Object? target, Type extensionType, [NotNullWhen(true)] out IExtension? extension)
        {
            extension = null;

            return (target as IExtensionCollection)?.TryGetExtension(extensionType, out extension) ?? false;
        }
        public static bool TryGetExtension<T>(UnityEngine.Object? target, [NotNullWhen(true)] out T? extension) where T : class, IExtension, new()
        {
            extension = null;

            return (target as IExtensionCollection)?.TryGetExtension<T>(out extension) ?? false;
        }
        public static IExtension? GetExtension(UnityEngine.Object? target, Type extensionType)
            => (target as IExtensionCollection)?.GetExtension(extensionType);
        public static T? GetExtension<T>(UnityEngine.Object? target) where T : class, IExtension, new()
            => (target as IExtensionCollection)?.GetExtension<T>();
    }
}
