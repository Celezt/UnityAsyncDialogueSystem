using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class AwaitTag : TagMarker
    {
        [Implicit]
        public float Duration { get; set; }

        [SerializeField]
        private float _someValue;

        public override void OnInvoke(int index, DSPlayableAsset playableAsset)
        {
            
        }
    }
}
