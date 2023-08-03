using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class WaitTag : DSTagSingle
    {
        [Implicit]
        public float Duration { get; set; }

        [SerializeField]
        private string _someValue;

        public override void OnInvoke(int index, DialogueAsset asset)
        {
            Debug.Log("AWAKE! " + index);
        }
    }
}
