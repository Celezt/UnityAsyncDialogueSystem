using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagSpan<T> : TagSpan
    {
        [NonSerialized]
        public new T? Binder => (T?)base.Binder;

        public virtual void OnCreate(RangeInt range, T? binder) { }
        public virtual void OnEnter(RangeInt range, T? binder) { }
        public virtual void OnProcess(int index, RangeInt range, T? binder) { }
        public virtual void OnExit(RangeInt range, T? binder) { }

        public sealed override void Initialize(RangeInt range, object? binder)
            => base.Initialize(range, binder);

        public sealed override void OnCreate() => OnCreate(Range, Binder);
        public sealed override void OnEnter() => OnEnter(Range, Binder);
        public sealed override void OnProcess(int index) => OnProcess(index, Range, Binder);
        public sealed override void OnExit() => OnExit(Range, Binder);
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

        public abstract void OnEnter();
        public abstract void OnProcess(int index);
        public abstract void OnExit();

        public override string ToString() => $"{GetType().Name.TrimDecoration("Tag").ToKebabCase()} {{ {Range.start}, {Range.end} }}";
    }
}
