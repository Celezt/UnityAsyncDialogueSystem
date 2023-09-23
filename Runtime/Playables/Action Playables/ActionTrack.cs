using Celezt.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.7f, 0.2f, 0.2f)]
    [TrackClipType(typeof(ActionAsset))]
    [TrackBindingType(typeof(ActionReceiver))]
    public class ActionTrack : ETrackAsset
    {
        protected ActionReceiver _receiver;
        protected override EMixerBehaviour CreateTrackMixer(PlayableGraph graph, PlayableDirector director, GameObject go, int inputCount)
        {
            GetReceiver(graph);

            var template = new ActionMixerBehaviour();

            return template;
        }

        private void GetReceiver(in PlayableGraph graph)
        {
            if (_receiver != null)
                return;

            if (_director == null)
                _director = graph.GetResolver() as PlayableDirector;

            if (_director.GetGenericBinding(this) is ActionReceiver receiver)
                _receiver = receiver;
            else if (_director.TryGetComponent(out receiver))
            {
                _director.SetGenericBinding(this, receiver);
                _receiver = receiver;
            }
            else
            {
                _receiver = _director.gameObject.AddComponent<ActionReceiver>();
                _director.SetGenericBinding(this, _receiver);
            }
        }
    }
}
