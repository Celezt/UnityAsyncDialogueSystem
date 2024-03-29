using Celezt.Timeline;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ActionEventBehaviour : EPlayableBehaviour
    {
        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            ActionEventAsset asset = Asset as ActionEventAsset;
            if (!asset.Receiver.ActionBinderDictionary.ContainsKey(asset))
                asset.Receiver.ActionBinderDictionary[Asset] = new ActionReceiver.ActionBinder { OnEnter = new UnityEvent(), OnExit = new UnityEvent()};  
        }

        public override void OnEnter(Playable playable, FrameData info, float weight, object playerData)
        {
            ActionEventAsset asset = Asset as ActionEventAsset;
            if (asset.Receiver.ActionBinderDictionary.TryGetValue(Asset, out var value))
                value.OnEnter.Invoke();
        }

        public override void OnExited(Playable playable, FrameData info, float weight, object playerData)
        {
            ActionEventAsset asset = Asset as ActionEventAsset;
            if (asset.Receiver.ActionBinderDictionary.TryGetValue(Asset, out var value))
                value.OnExit.Invoke();
        }
    }
}