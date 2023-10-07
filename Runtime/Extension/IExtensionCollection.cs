using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface IExtensionCollection : IEnumerable<IExtension>
    {
        public IReadOnlyList<IExtension> Extensions { get; }

        public void AddExtension(IExtensionCollection? extensions);
        public void AddExtension(IExtension? extension, UnityEngine.Object? reference = null);
        public void RemoveExtension(Type type);

        public void MoveUpExtension(Type type);
        public void MoveDownExtension(Type type);

        public bool TryGetExtension<T>([NotNullWhen(true)] out T? extension) where T : class
        {
            extension = null;

            if (TryGetExtension(typeof(T), out var e))
                extension = e as T;

            return extension != null;
        }
        public bool TryGetExtension(Type type, [NotNullWhen(true)] out IExtension? extension)
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
    }
}
