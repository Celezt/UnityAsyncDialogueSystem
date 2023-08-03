using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagSpan : ITag
    {
        public RangeInt Range { get; }

        public void Awake(RangeInt range, object? bind);
    }
}