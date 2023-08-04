using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public abstract class Tag : ITag
    {
        [NonSerialized]
        public object? Binder { get; protected set; }
    }
}
