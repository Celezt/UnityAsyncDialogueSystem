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
                if (clip.asset is DSPlayableAsset)
                {
                    DSPlayableAsset asset = clip.asset as DSPlayableAsset;
                    asset.Clip = clip;
                    asset.Director = _director;

                    DSPlayableBehaviour behaviour = null;
                    if (asset.BehaviourReference == null)
                        behaviour = asset.Initialization(graph, go);
                    else
                        behaviour = asset.BehaviourReference;

                    behaviour.Director = _director;
                    behaviour.Asset = asset;
                    behaviour.Clip = clip;

                    clip.displayName = asset.name;

                    behaviour.OnCreateTrackMixer(graph, go, clip);

                    if (_pendingOnCreate.Contains(asset))
                    {
                        behaviour.OnCreateClip();
                        _pendingOnCreate.Remove(asset);
                    }
                }
            }

            return ScriptPlayable<DSMixerBehaviour>.Create(graph, template, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            if (clip.asset is DSPlayableAsset asset)    // Waiting on being created.
                _pendingOnCreate.Add(asset);
        }
    }
}
