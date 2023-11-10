using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [CreateTag(int.MinValue)]
    public class WaitTag : TagSingle<ITimeIndex>
    {
        [Implicit]
        public float Duration { get; set; }

        public override void OnCreate(int index, ITimeIndex binder)
        {
            Debug.Assert(Duration >= 0, "Duration cannot be negative.");
            Duration = Mathf.Max(Duration, 0);
        }
    }

    [CreateTagSystem]
    public class WaitTagSystem : ITagSystem<WaitTag, ITimeIndex>
    {
        public void OnCreate(IReadOnlyList<WaitTag> entities, ITimeIndex binder)
        {
            float waitDuration = 0;

            foreach (WaitTag entity in entities)
                waitDuration += entity.Duration;


            double startTime = binder.StartTime + binder.StartOffset;
            float timeDuration = binder.TimeDurationWithoutOffset;
            float shrankTimeDuration = timeDuration - waitDuration;
            float offsetTime = 0;
            int offsetIndex = 0;
            int offsetCount = 0;
            int count = 0;

            if (waitDuration >= timeDuration)
                throw new Exception($"Combined wait is greater or equal to the duration: {waitDuration}s.");

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

                float timeInterval = (float)(time - startTime) / timeDuration;

                //
                //  Before Points
                //
                for (; count < keys.Length && keys[count].time <= timeInterval; count++)
                {
                    Keyframe key = keys[count];
                    Scale(ref key, shrankTimeDuration / timeDuration);
                    key.time += offsetTime / timeDuration;
                    newKeys[count + offsetIndex] = key;
                }

                //
                //  Wait Points
                //
                float tangent = binder.GetTangentByTime(time, CurveType.Editor);

                // First
                Keyframe firstKey = new Keyframe(timeInterval, visibility, tangent, 0);
                Scale(ref firstKey, shrankTimeDuration / timeDuration);
                firstKey.time += offsetTime / timeDuration;

                offsetIndices[offsetCount++] = count + offsetIndex;
                newKeys[count + offsetIndex++] = firstKey;

                offsetTime += entity.Duration;

                // Second
                Keyframe secondKey = new Keyframe(timeInterval, visibility, 0, tangent);
                Scale(ref secondKey, shrankTimeDuration / timeDuration);
                secondKey.time += offsetTime / timeDuration;

                offsetIndices[offsetCount++] = count + offsetIndex;
                newKeys[count + offsetIndex++] = secondKey;
            }

            //
            //  After Points
            //
            for (; count < keys.Length; count++)
            {
                Keyframe key = keys[count];
                Scale(ref key, shrankTimeDuration / timeDuration);
                key.time += offsetTime / timeDuration;
                newKeys[count + offsetIndex] = key;
            }

            binder.RuntimeVisibilityCurve.keys = newKeys;
        }

        private static void Scale(ref Keyframe key, float scaleX)
        {
            key.time *= scaleX;
            key.inTangent *= 1.0f / scaleX;
            key.outTangent *= 1.0f / scaleX;
        }
    }
}
