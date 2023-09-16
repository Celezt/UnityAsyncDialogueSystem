using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public interface ITime
    {
        public AnimationCurve EditorVisibilityCurve { get; }
        public AnimationCurve RuntimeVisibilityCurve { get; }
        public double Time { get; }
        public double FrameRate { get; }
        public double StartTime { get; }
        public double EndTime { get; }
        public float StartOffset { get; set; }
        public float EndOffset { get; set; }

        public float Tangent => GetTangentByTime(Time);

        /// <summary>
        /// How much time has passed in unit interval [0-1].
        /// </summary>
        public float Interval => GetIntervalByTime(Time);

        /// <summary>
        /// The ratio of visible characters in unit interval [0-1].
        /// </summary>
        public float VisibilityInterval => GetVisibilityByTime(Time);

        public float TimeDurationWithoutOffset => (float)(EndTime - StartTime) - EndOffset - StartOffset;

        public float GetVisibilityByTime(double time, CurveType curveType = CurveType.Runtime)
            => GetVisibilityByTimeInterval(GetIntervalByTime(time), curveType);

        public float GetIntervalByTime(double time) => Mathf.Clamp01((float)((time - StartTime - StartOffset) / TimeDurationWithoutOffset));

        public float GetVisibilityByTimeInterval(float timeInterval, CurveType curveType = CurveType.Runtime) => curveType switch
        {
            CurveType.Editor => EditorVisibilityCurve.Evaluate(timeInterval),
            CurveType.Runtime => RuntimeVisibilityCurve.Evaluate(timeInterval),
            _ => throw new NotSupportedException($"{curveType} is not supported."),
        };

        public float GetTangentByTime(double time, CurveType curveType = CurveType.Runtime)
        {
            float x1 = GetIntervalByTime(time - 0.001);
            float x2 = GetIntervalByTime(time + 0.001);
            float y1 = GetVisibilityByTimeInterval(x1, curveType);
            float y2 = GetVisibilityByTimeInterval(x2, curveType);
            return (y2 - y1) / (x2 - x1);
        }
    }
}
