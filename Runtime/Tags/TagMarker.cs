using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagMarker : ITag
    {
        public virtual void Initialize() { }
        public virtual void OnInvoke(int index, DSPlayableAsset playableAsset) { }
    }
}
