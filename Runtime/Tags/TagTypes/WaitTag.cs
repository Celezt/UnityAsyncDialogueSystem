using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            float duration = 0;

            foreach (WaitTag entity in entities)
                duration += entity.Duration;

            double startTime = binder.StartTime + binder.StartOffset;
            float timeDuration = binder.TimeDuration;
            float shrinkedTimeDuration = timeDuration - duration;
            float offsetTime = 0;
            int offsetIndex = 0;
            int offsetCount = 0;
            int count = 0;

            Span<int> offsetIndices = stackalloc int[entities.Count * 2];
            Keyframe[] keys = binder.EditorVisibilityCurve.keys;
            Keyframe[] newKeys = new Keyframe[binder.EditorVisibilityCurve.keys.Length + entities.Count * 2];

            foreach (WaitTag entity in entities)
            {
                if (entity.Duration <= 0)
                    continue;

                if (!binder.TryGetTimeByIndex(entity.Index, startTime, timeDuration, out double time, out float visibility,
                    curveType: CurveType.Editor))
                    continue;

                float ScaledTime() => (shrinkedTimeDuration + offsetTime) / timeDuration;

                for (; count < keys.Length && keys[count].time * timeDuration <= time; count++)
                {
                    Keyframe key = keys[count];
                    Scale(ref key, (shrinkedTimeDuration + offsetTime) / timeDuration);
                    newKeys[count + offsetIndex] = key;
                }

                //{
                //    float inTagent = keys[count - 1].inTangent;
                //    Keyframe key = new Keyframe((timeDuration - (float)time * (2.0f - ScaledTime())) / timeDuration, visibility, inTagent, 0);
                //    Scale(ref key, (shrinkedTimeDuration + offsetTime) / timeDuration);
                //    Debug.Log("first: " + key.time + " " + time); 
                //    offsetIndices[offsetCount++] = count + offsetIndex;
                //    newKeys[count + offsetIndex++] = key;
                //}

                offsetTime += entity.Duration;

                //{
                //    Keyframe key = new Keyframe((timeDuration - (float)time + offsetTime) / timeDuration, visibility, 0, 0);
                //    Scale(ref key, (shrinkedTimeDuration + offsetTime) / timeDuration);
                //    Debug.Log("second: " + key.time + " " + (shrinkedTimeDuration + offsetTime) / timeDuration);
                //    offsetIndices[offsetCount++] = count + offsetIndex;
                //    newKeys[count + offsetIndex++] = key;
                //}
            }

            for (; count < keys.Length; count++)
            {
                Keyframe key = keys[count];
                Scale(ref key, (shrinkedTimeDuration + offsetTime) / timeDuration);
                newKeys[count + offsetIndex] = key;
            }

            binder.RuntimeVisibilityCurve.keys = newKeys;

            for (int i = 0; i < offsetIndices.Length; i++)
            {
                if (i % 2 == 0)
                    AnimationUtility.SetKeyLeftTangentMode(
                        binder.RuntimeVisibilityCurve, offsetIndices[i], AnimationUtility.TangentMode.Linear);
                else
                    AnimationUtility.SetKeyRightTangentMode(binder.RuntimeVisibilityCurve,
                        offsetIndices[i], AnimationUtility.TangentMode.Linear);
            }

            foreach (Keyframe key in binder.RuntimeVisibilityCurve.keys)
            {
                //Debug.Log(key.time + " " + key.value);
            }
        }

        public static void Scale(ref Keyframe key, float scaleX)
        {
            key.time *= scaleX;
            key.inTangent *= 1.0f / scaleX;
            key.outTangent *= 1.0f / scaleX;
        }

        public static void ScaleCurve(Span<Keyframe> keys, float scaleX, float scaleY)
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
