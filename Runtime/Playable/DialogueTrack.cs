using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.7f, 0.2f, 0.2f)]
    [TrackClipType(typeof(DSPlayableAsset))]
    public class DialogueTrack : TrackAsset
    {
        private DSMixerBehaviour _template = new DSMixerBehaviour();
        private PlayableDirector _director;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            _director = graph.GetResolver() as PlayableDirector;

            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is DSPlayableAsset)
                {
                    DSPlayableAsset asset = clip.asset as DSPlayableAsset;
                    DSPlayableBehaviour behaviour = asset.BehaviourReference;
                    behaviour.Director = _director;
                    behaviour.Asset = asset;

                    clip.displayName = asset.name;

                    behaviour.OnCreateTrackMixer(graph, go, inputCount);

                    behaviour.StartTime = clip.start;
                    behaviour.EndTime = clip.end;   
                }
            }

            return ScriptPlayable<DSMixerBehaviour>.Create(graph, _template, inputCount);
        }
    }
}
