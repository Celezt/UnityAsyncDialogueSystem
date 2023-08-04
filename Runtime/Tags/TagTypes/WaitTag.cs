using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class WaitTag : TagSingle<DialogueAsset>
    {
        [Implicit]
        public float Duration { get; set; }

        [SerializeField]
        private string _someValue;
    }

    [CreateTagSystem]
    public class WaitTagSystem : ITagSystem<WaitTag>
    {
        public void Execute(IReadOnlyList<WaitTag> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                WaitTag tag = entities[i];
                //Keyframe keyframe = new(index / asset.Length, 0, 0, 0);

                //tag.Asset.RuntimeVisibilityCurve.AddKey(keyframe);
            }
        }
    }
}
