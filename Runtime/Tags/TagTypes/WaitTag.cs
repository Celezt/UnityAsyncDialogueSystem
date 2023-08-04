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
    public class WaitTagSystem : ITagSystem<WaitTag, DialogueAsset>
    {
        public void OnCreate(IReadOnlyList<WaitTag> entities, DialogueAsset binder)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                WaitTag tag = entities[i];
                //ScaleCurve(tag)
                //Keyframe keyframe = new(index / asset.Length, 0, 0, 0);

                //tag.Asset.RuntimeVisibilityCurve.AddKey(keyframe);
            }
        }

        public static void ScaleCurve(Keyframe[] keys, float scaleX, float scaleY)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                Keyframe keyframe = keys[i];
                keyframe.value = keys[i].value * scaleY;
                keyframe.time = keys[i].time * scaleX;
                keyframe.inTangent = keys[i].inTangent * scaleY / scaleX;
                keyframe.outTangent = keys[i].outTangent * scaleY / scaleX;
                keys[i] = keyframe;
            }
        }
    }
}
