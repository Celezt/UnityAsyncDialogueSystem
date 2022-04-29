using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem 
{
    /// <summary>
    /// A base class for assets that can be used to instantiate a Playable at runtime.
    /// </summary>
    public abstract class DSPlayableAsset: PlayableAsset, ITimelineClipAsset
    {
        public abstract new string name { get; }
        public abstract ClipCaps clipCaps { get; }
        public abstract DSPlayableBehaviour BehaviourReference { get; }

    }

    /// <summary>
    /// A base class for assets that can be used to instantiate a Playable at runtime.
    /// </summary>
    public abstract class DSPlayableAsset<T> : DSPlayableAsset, ITimelineClipAsset where T : DSPlayableBehaviour, new()
    {
        public override string name => GetType().Name.Replace("Asset", "");
        public override ClipCaps clipCaps => ClipCaps.None;

        public sealed override DSPlayableBehaviour BehaviourReference => _template;

        [SerializeReference, HideInInspector]
        private T _template = new T();

        public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<DSPlayableBehaviour>.Create(graph, _template);
        }
    }
}
