using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

#nullable enable

namespace Celezt.DialogueSystem
{
    public static class TimelineExtensions
    {
        /// <summary>
        /// Find track with available space.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// <param name="timeline">Find in timeline.</param>
        /// <param name="start">Minimum position.</param>
        /// <param name="reversed">If searching for the last track.</param>
        /// <returns>The available track. If not, return null.</returns>
        public static T? FindTrackSpace<T>(this TimelineAsset timeline, double start, bool reversed = false) where T : UnityEngine.Timeline.TrackAsset, new()
            => FindTrackSpace<T>(timeline.GetOutputTracks().OfType<T>(), start, reversed);

        /// <summary>
        /// Find track with available space.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// <param name="tracks">Find in tracks.</param>
        /// <param name="start">Minimum position.</param>
        /// <param name="reversed">If searching for the last track.</param>
        /// <returns>The available track. If not, return null.</returns>
        public static T? FindTrackSpace<T>(IEnumerable<T> tracks, double start, bool reversed = false) where T : UnityEngine.Timeline.TrackAsset, new()
        {
            return reversed ? tracks.LastOrDefault(x => x.end < start) : tracks.FirstOrDefault(x => x.end < start); // Get the first or last instance.
        }

        /// <summary>
        /// Find track with available space or allocate new track of that type.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// <param name="timeline">Find in timeline.</param>
        /// <param name="start">Minimum position.</param>
        /// <param name="reversed">If searching for the last track.</param>
        /// <returns>The available track.</returns>
        public static T FindOrAllocateTrackSpace<T>(this TimelineAsset timeline, double start, bool reversed = false) where T : UnityEngine.Timeline.TrackAsset, new()
        {
            T? track = FindTrackSpace<T>(timeline, start, reversed);

            if (track == null)
                track = timeline.CreateTrack<T>();

            return track;
        }

        /// <summary>
        /// Find track or create new track of that type.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// /// <param name="timeline">Find in timeline.</param>
        /// <returns>The available track or new track</returns>
        public static T? FindOrCreate<T>(this TimelineAsset timeline) where T : UnityEngine.Timeline.TrackAsset, new()
        {
            return timeline.GetOutputTracks().OfType<T>().FirstOrDefault() ?? timeline.CreateTrack<T>();
        }

        /// <summary>
        /// Find the index.
        /// </summary>
        /// <param name="timeline">Find in timeline.</param>
        /// <param name="track">Track to find the index for.</param>
        /// <returns>Index. -1 if not found.</returns>
        public static int IndexOf(this TimelineAsset timeline, UnityEngine.Timeline.TrackAsset track)
        {
            return timeline.GetOutputTracks().IndexOf(track);
        }
    }
}