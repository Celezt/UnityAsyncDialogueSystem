using Celezt.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public abstract class DSTrackAsset : ETrackAsset
    {
        protected DialogueSystemBinder _binder;

        protected override EMixerBehaviour CreateTrackMixer(PlayableGraph graph, PlayableDirector director, GameObject go, int inputCount)
        {
            GetBinder(graph);

            var template = new DSMixerBehaviour();

            return template;
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
        }
    }
}
