using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class AwaitTag : TagMarker
    {
        [Implicit]
        public float Speed { get; set; }

        public override void OnInvoke(int index, DSPlayableAsset playableAsset)
        {
            
        }
    }
}
