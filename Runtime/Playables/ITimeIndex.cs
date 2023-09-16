using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public interface ITimeIndex : ITime
    {
        public int Length { get; }

        public int Index => GetIndexByTime(Time);

        public int GetIndexByTime(double time, CurveType curveType = CurveType.Runtime) => (int)Mathf.Round(GetVisibilityByTime(time, curveType) * Length);

        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, out double time, CurveType curveType = CurveType.Runtime)
            => TryGetTimeByIndex(index, out time, out _, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, double startTime, double duration, out double time, CurveType curveType = CurveType.Runtime)
            => TryGetTimeByIndex(index, startTime, duration, out time, out _, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, out double time, out float visibility, CurveType curveType = CurveType.Runtime)
            => TryGetTimeByIndex(index, StartTime + StartOffset, TimeDurationWithoutOffset, out time, out visibility, curveType);
        /// <summary>
        /// Try get first intersection by index.
        /// </summary>
        /// <returns>If any intersections exist.</returns>
        public bool TryGetTimeByIndex(int index, double startTime, double duration, out double time, out float visibility, CurveType curveType = CurveType.Runtime)
        {
            double framerate = FrameRate;
            float previousValue = 0;
            time = startTime;
            visibility = 0;

            while (time < startTime + duration)
            {
                visibility = ((ITime)this).GetVisibilityByTime(time, curveType);
                float currentValue = visibility * Length;

                if (index == 0 && StartOffset > 0 ? previousValue <= index && currentValue > index :
                                                    previousValue < index && currentValue >= index)
                    return true;

                time += 1.0 / framerate;
                previousValue = currentValue;
            }

            return false;
        }
        /// <summary>
        /// Get all intersections by index.
        /// </summary>
        /// <returns>Intersection count.</returns>
        public int GetTimeByIndex(int index, double startTime, double duration, IList<double> intersections, CurveType curveType = CurveType.Runtime)
        {
            double framerate = FrameRate;
            float previousValue = 0;
            double time = startTime;
            int count = 0;

            while (time < startTime + duration && intersections.Count < count)
            {
                float visibility = ((ITime)this).GetVisibilityByTime(time, curveType);
                float currentValue = visibility * Length;

                if (index == 0 && StartOffset > 0 ? previousValue <= index && currentValue > index :
                                                    previousValue < index && currentValue >= index)
                    intersections[count++] = time;

                time += 1.0 / framerate;
            }

            return count;
        }
    }
}
