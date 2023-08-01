using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem
{
    public class TagElement : ITag
    {
        public RangeInt Range => _range;

        internal RangeInt _range;

        public virtual void OnCreate() { }
        public virtual void OnEnter(int index, RangeInt range, DSPlayableAsset playableAsset) { }
        public virtual void OnProcess(int index, RangeInt range, DSPlayableAsset playableAsset) { }
        public virtual void OnExit(int index, RangeInt range, DSPlayableAsset playableAsset) { }
    }
}
