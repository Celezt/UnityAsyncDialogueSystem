using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface ITagSingle : ITag
    {
        public int Index { get; }

        public void Awake(int index, object? bind);
    }
}
