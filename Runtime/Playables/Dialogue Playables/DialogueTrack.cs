using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.2f, 0.4f, 0.5f)]
    [TrackClipType(typeof(DialogueAsset))]
    [TrackBindingType(typeof(DialogueSystemBinder))]
    public class DialogueTrack : DSTrack
    {
        protected DialogueSystemBinder _binder;

        protected override DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, PlayableDirector director, GameObject go, int inputCount)
        {
            GetBinder(graph);

            var template = new DialogueMixerBehaviour();
            template.Binder = _binder;

            return template;
        }

        private void OnDestroy()
        {
            _binder?.Internal_Remove(this);
        }

        private void GetBinder(in PlayableGraph graph)
        {
            if (_binder != null)
                return;

            if (_director == null)
                _director = graph.GetResolver() as PlayableDirector;

            if (_director.GetGenericBinding(this) is DialogueSystemBinder binder)
                _binder = binder;
            else if (_director.TryGetComponent(out binder))
            {
                _director.SetGenericBinding(this, binder);
                _binder = binder;
            }
            else
            {
                _binder = _director.gameObject.AddComponent<DialogueSystemBinder>();
                _director.SetGenericBinding(this, _binder);
            }

            _binder.Internal_Add(this);
        }
    }
}
