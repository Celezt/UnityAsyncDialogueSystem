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

        public override void OnCreate(int index, DialogueAsset asset)
        {
            Keyframe keyframe = new(index / asset.Length, 0, 0, 0);

            asset.RuntimeVisibilityCurve.AddKey(keyframe);
        }
    }

    [CreateTagSystem]
    public class WaitTagSystem : ITagSystem<WaitTag>
    {
        public void Execute(IReadOnlyList<WaitTag> entities)
        {
            Debug.Log(entities.Count);
        }
    }
}
