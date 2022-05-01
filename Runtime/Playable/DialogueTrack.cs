using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.7f, 0.2f, 0.2f)]
    [TrackClipType(typeof(DSPlayableAsset))]
    [TrackBindingType(typeof(DialogueSystemBinder))]
    public class DialogueTrack : TrackAsset
    {
        public DSMixerBehaviour Mixer { get; internal set; }

        private DialogueSystemBinder _binder;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var director = graph.GetResolver() as PlayableDirector;
            _binder = director.GetGenericBinding(this) as DialogueSystemBinder;

            DSMixerBehaviour template = new DSMixerBehaviour();

            if (_binder != null)
            {
                _binder.Director = director;
                template.Binder = _binder.Add(this);
                template.Track = this;
            }

            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is DSPlayableAsset)
                {
                    DSPlayableAsset asset = clip.asset as DSPlayableAsset;
                    DSPlayableBehaviour behaviour = asset.BehaviourReference;
                    behaviour.Director = director;
                    behaviour.Asset = asset;

                    clip.displayName = asset.name;

                    behaviour.OnCreateTrackMixer(graph, go, inputCount, clip);

                    behaviour.StartTime = clip.start;
                    behaviour.EndTime = clip.end;   
                }
            }

            return ScriptPlayable<DSMixerBehaviour>.Create(graph, template, inputCount);
        }


        private void OnDestroy()
        {
            _binder.Remove(this);
        }
    }
}
