using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagSingle<T> : TagSingle where T : new()
    {
        [NonSerialized]
        public new T? Binder => (T?)base.Binder;

        public virtual void OnCreate(int index, T? binder) { }
        public virtual void OnInvoke(int index, T? binder) { }

        public sealed override void Initialize(int index, object? binder)
            => base.Initialize(index, binder);

        public sealed override void OnCreate() => OnCreate(Index, Binder);
        public sealed override void OnInvoke() => OnInvoke(Index, Binder);
    }

    public abstract class TagSingle : Tag, ITagSingle
    {
        [NonSerialized]
        public int Index { get; protected set; }

        public virtual void Initialize(int index, object? binder)
        {
            Binder = binder;
            Index = index;
        }

        public abstract void OnInvoke();

        public override string ToString() => $"{GetType().Name.TrimDecoration("Tag").ToKebabCase()} {{ {Index} }}";
    }
}
