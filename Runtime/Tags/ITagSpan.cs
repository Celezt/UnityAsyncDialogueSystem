using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagSpan : ITag
    {
        public RangeInt Range { get; }

        public void Initialize(RangeInt range, object? bind);
        public void OnEnter();
        public void OnProcess(int index);
        public void OnExit();
    }
}
