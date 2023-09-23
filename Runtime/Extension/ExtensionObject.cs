using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    [CreateAssetMenu(fileName = "New Extension", menuName = "Extension")]
    public class ExtensionObject : ScriptableObject, IExtensionCollection
    {
        public IReadOnlyList<IExtension> Extensions => _extensions;

        [SerializeReference]
        private List<IExtension> _extensions = new();

        public void AddExtension(IExtensionCollection? extensions)
             => ExtensionUtility.AddExtensions(this, extensions, _extensions);

        public void AddExtension(IExtension? extension, UnityEngine.Object? reference = null)
            => ExtensionUtility.AddExtension(this, extension, _extensions, reference);

        public void RemoveExtension(Type type)
            => ExtensionUtility.RemoveExtension(type, _extensions);

        public void MoveUpExtension(Type type)
            => ExtensionUtility.MoveUpExtension(type, _extensions);
        public void MoveDownExtension(Type type)
            => ExtensionUtility.MoveDownExtension(type, _extensions);

        public bool TryGetExtension<T>(out T? extension) where T : class
        {
            extension = null;

            if (TryGetExtension(typeof(T), out var e))
                extension = e as T;

            return extension != null;
        }
        public bool TryGetExtension(Type type, out IExtension? extension)
        {
            extension = Extensions.FirstOrDefault(x => x.GetType().IsAssignableFrom(type));

            return extension != null;
        }
        public IExtension? GetExtension(Type type)
        {
            if (TryGetExtension(type, out var extension))
                return extension;

            return null;
        }

        public bool Contains(Type type)
            => Extensions.Any(x => type.IsAssignableFrom(x.GetType()));

        IEnumerator<IExtension> IEnumerable<IExtension>.GetEnumerator() => _extensions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _extensions.GetEnumerator();
    }
}
