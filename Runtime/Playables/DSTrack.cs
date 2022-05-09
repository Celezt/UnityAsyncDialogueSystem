using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public abstract class DSTrack : TrackAsset
    {
        public DSMixerBehaviour Mixer { get; internal set; }

        protected PlayableDirector _director;

        private HashSet<DSPlayableAsset> _pendingOnCreate = new HashSet<DSPlayableAsset>();

        protected virtual DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, PlayableDirector director, GameObject go, int inputCount) => new DSMixerBehaviour();

        public sealed override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            if (_director == null)
                _director = graph.GetResolver() as PlayableDirector;

            DSMixerBehaviour template = CreateTrackMixer(graph, _director, go, inputCount);
            template.Track = this;

            foreach (TimelineClip clip in GetClips())
            {
                UpdateClip(graph, clip);
            }

            return ScriptPlayable<DSMixerBehaviour>.Create(graph, template, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            if (clip.asset is DSPlayableAsset asset)    // Waiting on being created.
                _pendingOnCreate.Add(asset);
        }

        public void UpdateClip(TimelineClip clip) => UpdateClip(_director.playableGraph, clip);
        public void UpdateClip(PlayableGraph graph, TimelineClip clip)
        {
            if (clip.asset is DSPlayableAsset)
            {
                DSPlayableAsset asset = clip.asset as DSPlayableAsset;
                
                DSPlayableBehaviour behaviour = null;
                if (asset.BehaviourReference == null)
                    behaviour = asset.Initialization(graph, _director.gameObject);
                else
                    behaviour = asset.BehaviourReference;

                behaviour.Director = _director;
                behaviour.Asset = asset;
                behaviour.Clip = clip;

                clip.displayName = asset.name;

                behaviour.OnCreateTrackMixer(graph, _director.gameObject, clip);

                if (_pendingOnCreate.Contains(asset))
                {
                    behaviour.OnCreateClip();
                    _pendingOnCreate.Remove(asset);
                }
            }
        }
    }
}
