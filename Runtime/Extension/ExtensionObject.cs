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

        public bool Contains(Type type) => _extensions.Any(x => type.IsAssignableFrom(x.GetType())); 

        IEnumerator<IExtension> IEnumerable<IExtension>.GetEnumerator() => _extensions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _extensions.GetEnumerator();
    }
}
