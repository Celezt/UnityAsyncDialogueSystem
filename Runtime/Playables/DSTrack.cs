using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackBindingType(typeof(DialogueSystemBinder))]
    public abstract class DSTrack : TrackAsset
    {
        public DSMixerBehaviour Mixer { get; internal set; }
        public DialogueSystemBinder Binder => _binder;
        public PlayableDirector Director => _director;

        private DialogueSystemBinder _binder;
        private PlayableDirector _director;

        protected virtual DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, DialogueSystemBinder binder, PlayableDirector director, GameObject go, int inputCount) => new DSMixerBehaviour();

        public sealed override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            _director = graph.GetResolver() as PlayableDirector;

            GetBinder(graph);

            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is DSPlayableAsset)
                {
                    DSPlayableAsset asset = clip.asset as DSPlayableAsset;
                    DSPlayableBehaviour behaviour = asset.BehaviourReference;
                    behaviour.Director = _director;
                    behaviour.Asset = asset;
                    behaviour.Clip = clip;
                    behaviour.Binder = _binder;

                    clip.displayName = asset.name;

                    behaviour.OnCreateTrackMixer(graph, go, inputCount, clip);
                }
            }

            DSMixerBehaviour template = CreateTrackMixer(graph, _binder, _director, go, inputCount);
            template.Binder = _binder;
            template.Track = this;

            return ScriptPlayable<DSMixerBehaviour>.Create(graph, template, inputCount);
        }

        protected sealed override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            GetBinder(graph);

            return base.CreatePlayable(graph, gameObject, clip);
        }

        private void OnDestroy()
        {
            _binder?.Remove(this);
        }

        private void GetBinder(PlayableGraph graph)
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

            if (_binder != null)
            {
                _binder.Director = _director;
                _binder.Add(this);
            }

        }
    }
}
