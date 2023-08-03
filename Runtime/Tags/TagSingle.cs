using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
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
