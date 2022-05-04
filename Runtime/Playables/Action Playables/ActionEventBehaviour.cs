using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ActionEventBehaviour : DSPlayableBehaviour
    {
        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount, TimelineClip clip)
        {
            if (!Binder.ActionBinderDictionary.ContainsKey(Asset))
                Binder.ActionBinderDictionary[Asset] = new DialogueSystemBinder.ActionBinder { };  
        }

        public override void EnterClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            if (Binder.ActionBinderDictionary.TryGetValue(Asset, out var value))
                value.OnEnter.Invoke();
        }

        public override void ExitClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            if (Binder.ActionBinderDictionary.TryGetValue(Asset, out var value))
                value.OnExit.Invoke();
        }
    }
}