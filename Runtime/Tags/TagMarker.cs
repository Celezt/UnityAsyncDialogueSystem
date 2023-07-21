using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagMarker : ITag
    {
        public int Index => _index;

        internal int _index;

        public virtual void Initialize() { }
        public virtual void OnInvoke(int index, DSPlayableAsset playableAsset) { }
    }
}
