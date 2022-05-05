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

        protected PlayableDirector _director;

        protected virtual DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, PlayableDirector director, GameObject go, int inputCount) => new DSMixerBehaviour();

        public sealed override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            if (_director == null)
                _director = graph.GetResolver() as PlayableDirector;


            DSMixerBehaviour template = CreateTrackMixer(graph, _director, go, inputCount);

            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is DSPlayableAsset)
                {
                    DSPlayableAsset asset = clip.asset as DSPlayableAsset;
                    DSPlayableBehaviour behaviour = asset.BehaviourReference;
                    behaviour.Director = _director;
                    behaviour.Asset = asset;
                    behaviour.Clip = clip;

                    clip.displayName = asset.name;

                    behaviour.OnCreateTrackMixer(graph, go, inputCount, clip);
                }
            }


            template.Track = this;

            return ScriptPlayable<DSMixerBehaviour>.Create(graph, template, inputCount);
        }
    }
}
