using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagSingle<T> : TagSingle where T : new()
    {
        public T? Asset => (T?)Bind;

        public virtual void OnCreate(int index, T? asset) { }
        public virtual void OnInvoke(int index, T? asset) { }

        public sealed override void Awake(int index, object? bind)
        {
            base.Awake(index, bind);

            OnCreate(index, (T?)bind);
        }

        internal void Internal_OnInvoke() => OnInvoke(Index, Asset);
    }

    public abstract class TagSingle : Tag, ITagSingle
    {
        public int Index { get; protected set; }

        public virtual void Awake(int index, object? bind)
        {
            Bind = bind;
            Index = index;
        }
    }
}
