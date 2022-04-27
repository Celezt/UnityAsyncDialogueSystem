using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem 
{
    public abstract class DialogueAsset: PlayableAsset, ITimelineClipAsset
    {
        public abstract new string name { get; }
        public abstract ClipCaps clipCaps { get; }
        public abstract DialogueBehaviour BehaviourReference { get; }
    }

    public abstract class DialogueAsset<T> : DialogueAsset, ITimelineClipAsset where T : DialogueBehaviour, new()
    {
        public override string name => GetType().Name.Replace("Asset", "");
        public override ClipCaps clipCaps => ClipCaps.None;

        public sealed override DialogueBehaviour BehaviourReference => _template;

        private T _template = new T();

        public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<DialogueBehaviour>.Create(graph, _template);
        }
    }
}
