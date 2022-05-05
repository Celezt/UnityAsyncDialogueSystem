using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.7f, 0.2f, 0.2f)]
    [TrackClipType(typeof(ActionEventAsset))]
    [TrackClipType(typeof(ButtonAsset))]
    [TrackBindingType(typeof(ActionReceiver))]
    public class ActionTrack : DSTrack
    {
        protected ActionReceiver _receiver;
        protected override DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, PlayableDirector director, GameObject go, int inputCount)
        {
            GetReceiver(graph);

            var template = new ActionMixerBehaviour();

            return template;
        }

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            GetReceiver(graph);

            return base.CreatePlayable(graph, gameObject, clip);
        }

        private void GetReceiver(in PlayableGraph graph)
        {
            if (_receiver != null)
                return;

            if (_director == null)
                _director = graph.GetResolver() as PlayableDirector;

            if (_director.GetGenericBinding(this) is ActionReceiver reciver)
                _receiver = reciver;
            else if (_director.TryGetComponent(out reciver))
            {
                _director.SetGenericBinding(this, reciver);
                _receiver = reciver;
            }
            else
            {
                _receiver = _director.gameObject.AddComponent<ActionReceiver>();
                _director.SetGenericBinding(this, _receiver);
            }
        }
    }
}
