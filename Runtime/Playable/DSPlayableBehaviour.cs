using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    /// <summary>
    /// PlayableBehaviour is the base class from which every custom playable script derives.
    /// </summary>
    public class DSPlayableBehaviour : PlayableBehaviour
    {
        public bool IsPlaying
        {
            get
            {
                double time = Director.time;
                return time <= EndTime && time > StartTime;
            }
        }

        public DSPlayableAsset Asset { get; internal set; }
        public PlayableDirector Director { get; internal set; }
        public double StartTime { get; internal set; }
        public double EndTime { get; internal set; }
        /// <summary>
        /// Current process state of the clip.
        /// </summary>
        public ProcessStates ProcessState { get; internal set; }

        public enum ProcessStates
        {
            None,
            Processing,
        }

        /// <summary>
        /// This function is called during the CreateTrackMixer phase of the PlayableGraph.
        /// </summary>
        /// <param name="graph">The Graph that owns the current PlayableBehaviour.</param>
        /// <param name="go">The GameObject that the graph is connected to.</param>
        /// <param name="inputCount">Input count.</param>
        /// <param name="clip">The TimelineClip that owns the current PlayableBehaviour.</param>
        public virtual void OnCreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount, TimelineClip clip) { }
        /// <summary>
        /// This function is called during the ExitClip phase of the PlayableGraph.
        /// </summary>
        /// <param name="playable">The Playable that owns the current PlayableBehaviour.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        /// <param name="binder">The user data of the ScriptPlayableOutput that initiated the process pass.</param>
        public virtual void ExitClip(Playable playable, FrameData info, DialogueSystemBinder binder) { }
        /// <summary>
        /// This function is called during the EnterClip phase of the PlayableGraph.
        /// </summary>
        /// <param name="playable">The Playable that owns the current PlayableBehaviour.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        /// <param name="binder">The user data of the ScriptPlayableOutput that initiated the process pass.</param>
        public virtual void EnterClip(Playable playable, FrameData info, DialogueSystemBinder binder) { }
        /// <summary>
        /// This function is called during the ProcessFrame phase of the PlayableGraph.
        /// </summary>
        /// <param name="playable">The Playable that owns the current PlayableBehaviour.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        /// <param name="binder">The user data of the ScriptPlayableOutput that initiated the process pass.</param>
        public virtual void ProcessFrame(Playable playable, FrameData info, DialogueSystemBinder binder) { }

        public sealed override void ProcessFrame(Playable playable, FrameData info, object playerData) => ProcessFrame(playable, info, playerData as DialogueSystemBinder);
    }
}
