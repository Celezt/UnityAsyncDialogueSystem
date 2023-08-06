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
    public abstract class DSPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public new virtual string name => GetType().Name.Replace("Asset", "");
        public virtual ClipCaps clipCaps => ClipCaps.None;

        public bool IsReady { get; internal set; }
        public TimelineClip Clip { get; internal set; }
        public PlayableDirector Director { get; internal set; }

        public DSPlayableBehaviour BehaviourReference => _template;

        [SerializeReference, HideInInspector]
        private DSPlayableBehaviour _template;
        
        protected abstract DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner);
         
        public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<DSPlayableBehaviour>.Create(graph, _template);
        }

        internal DSPlayableBehaviour Initialization(PlayableGraph graph, GameObject owner)
        {
            _template = CreateBehaviour(graph, owner);
            return _template;
        }
    }
}
