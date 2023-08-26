using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagSpan<T> : TagSpan where T : new()
    {
        [NonSerialized]
        public new T? Binder => (T?)base.Binder;

        public virtual void OnCreate(RangeInt range, T? binder) { }
        public virtual void OnEnter(int index, RangeInt range, T? binder) { }
        public virtual void OnProcess(int index, RangeInt range, T? binder) { }
        public virtual void OnExit(int index, RangeInt range, T? binder) { }

        public sealed override void Initialize(RangeInt range, object? binder)
            => base.Initialize(range, binder);

        public sealed override void OnCreate() => OnCreate(Range, Binder);
        public void OnEnter(int index) => OnEnter(index, Range, Binder);
        public void OnProcess(int index) => OnProcess(index, Range, Binder);
        public void OnExit(int index) => OnExit(index, Range, Binder);
    }

    public abstract class TagSpan : Tag, ITagSpan
    {
        [NonSerialized]
        public RangeInt Range { get; protected set; }

        public virtual void Initialize(RangeInt range, object? binder)
        {
            Binder = binder;
            Range = range;
        }
    }
}
