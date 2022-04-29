using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    /// <summary>
    /// PlayableBehaviour is the base class from which every custom playable script derives.
    /// </summary>
    public class DSPlayableBehaviour : PlayableBehaviour
    {
        public DSPlayableAsset Asset { get; internal set; }
        public PlayableDirector Director { get; internal set; }
        public double StartTime { get; internal set; }
        public double EndTime { get; internal set; }

        /// <summary>
        /// This function is called during the CreateTrackMixer phase of the PlayableGraph.
        /// </summary>
        /// <param name="graph">The Graph that owns the current PlayableBehaviour.</param>
        /// <param name="go">The GameObject that the graph is connected to.</param>
        /// <param name="inputCount">Input count.</param>
        public virtual void OnCreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) { }
        /// <summary>
        /// This function is called during the PostFrame phase of the PlayableGraph.
        /// </summary>
        /// <param name="playable">The Playable that owns the current PlayableBehaviour.</param>
        /// <param name="info">A FrameData structure that contains information about the current frame context.</param>
        /// <param name="playerData">The user data of the ScriptPlayableOutput that initiated the process pass.</param>
        public virtual void PostFrame(Playable playable, FrameData info, object playerData) { }
    }
}
