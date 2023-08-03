using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
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
