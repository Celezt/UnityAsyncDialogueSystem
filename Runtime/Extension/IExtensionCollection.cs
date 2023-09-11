using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface IExtensionCollection : IEnumerable<IExtension>
    {
        public IReadOnlyList<IExtension> Extensions { get; }

        public void AddExtension(IExtensionCollection? extensions);
        public void AddExtension(IExtension? extension, UnityEngine.Object? reference = null);
        public void RemoveExtension(Type type);

        public bool Contains(Type type);
    }
}
