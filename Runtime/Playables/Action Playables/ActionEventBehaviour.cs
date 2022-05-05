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
        public ActionReceiver Receiver => _receiver;

        ActionReceiver _receiver;

        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount, TimelineClip clip)
        {
            GetReceiver(graph);
            if (!_receiver.ActionBinderDictionary.ContainsKey(Asset))
                _receiver.ActionBinderDictionary[Asset] = new ActionReceiver.ActionBinder { };  
        }

        public override void EnterClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            GetReceiver(playable.GetGraph());
            if (_receiver.ActionBinderDictionary.TryGetValue(Asset, out var value))
                value.OnEnter.Invoke();
        }

        public override void ExitClip(Playable playable, FrameData info, DialogueSystemBinder binder)
        {
            GetReceiver(playable.GetGraph());
            if (_receiver.ActionBinderDictionary.TryGetValue(Asset, out var value))
                value.OnExit.Invoke();
        }

        private void GetReceiver(in PlayableGraph graph)
        {
            if (_receiver != null)
                return;

            _receiver = Director.GetGenericBinding(Clip.GetParentTrack()) as ActionReceiver;
        }
    }
}