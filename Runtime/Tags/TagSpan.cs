using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagSpan<T> : TagSpan where T : new()
    {
        public T? Asset => (T?)Bind;

        public virtual void OnCreate(RangeInt range, T? asset) { }
        public virtual void OnEnter(int index, RangeInt range, T? asset) { }
        public virtual void OnProcess(int index, RangeInt range, T? asset) { }
        public virtual void OnExit(int index, RangeInt range, T? asset) { }

        public sealed override void Awake(RangeInt range, object? bind)
        {
            base.Awake(range, bind);

            OnCreate(range, (T?)bind);
        }

        internal void Internal_OnEnter(int index) => OnEnter(index, Range, Asset);
        internal void Internal_OnProcess(int index) => OnProcess(index, Range, Asset);
        internal void Internal_OnExit(int index) => OnExit(index, Range, Asset);
    }

    public abstract class TagSpan : Tag, ITagSpan
    {
        public RangeInt Range { get; protected set; }

        public virtual void Awake(RangeInt range, object? bind)
        {
            Bind = bind;
            Range = range;
        }
    }
}
