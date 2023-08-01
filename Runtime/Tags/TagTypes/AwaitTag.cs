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
        private string _someValue;

        public override void OnInvoke(int index, DSPlayableAsset playableAsset)
        {
            Debug.Log("AWAKE! " + index);
        }
    }
}
