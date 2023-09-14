using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public static class ExtensionUtility
    {
        public static void AddExtensions(UnityEngine.Object owner, IExtensionCollection? toAddCollection, IList<IExtension> extensions)
        {
            if (toAddCollection == null)
                return;

            UnityEngine.Object? reference = toAddCollection as UnityEngine.Object;
            if (reference)
            {
                if (reference == owner)
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
                AddExtension(owner, toAdd, extensions, reference);
        }

        public static void AddExtension(UnityEngine.Object owner, IExtension? toAdd, IList<IExtension> extensions, UnityEngine.Object? reference = null)
        {
            if (toAdd == null)
                return;

            Type type = toAdd.GetType();
            if (!extensions.Any(x => x.GetType() == type))  // Duplicates are not allowed.
            {
                toAdd.Reference = reference;

#if UNITY_EDITOR
                if (!HasSelfReference(owner, toAdd.Reference))
#endif
                    extensions.Add(toAdd);
            }
            else
                Debug.LogWarning("Duplicates are not allowed.");
        }

        public static void RemoveExtension(Type type, IList<IExtension> extensions)
        {
            int indexExtension = extensions.FindIndex(x => type.IsAssignableFrom(x.GetType()));

            if (indexExtension < 0)
                return;

            extensions.RemoveAt(indexExtension);
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
    }
}
