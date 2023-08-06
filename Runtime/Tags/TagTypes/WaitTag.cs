using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag]
    public class WaitTag : TagSingle<DialogueAsset>
    {
        [Implicit]
        public float Duration { get; set; }
    }

    [CreateTagSystem]
    public class WaitTagSystem : ITagSystem<WaitTag, DialogueAsset>
    {
        public void OnCreate(IReadOnlyList<WaitTag> entities, DialogueAsset binder)
        {
            Keyframe[] keys = binder.VisibilityCurve.keys;
            Span<(int, float)> values = stackalloc (int, float)[entities.Count];
            float duration = 0;

            for (int i = 0; i < values.Length; i++)
            {
                WaitTag tag = entities[i];
                values[i] = (tag.Index, tag.Duration);
                duration += tag.Duration;
            }
            //Debug.Log(duration + " " + binder.TimeLength + " " + ((binder.TimeLength - duration) / binder.TimeLength));
            ScaleCurve(keys, (binder.TimeLength - duration) / binder.TimeLength, 1.0f);

            binder.RuntimeVisibilityCurve.keys = keys;
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
