using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.7f, 0.2f, 0.2f)]
    [TrackClipType(typeof(DialogueAsset))]
    public class DialogueTrack : TrackAsset
    {
        public DialogueMixerBehaviour Template = new DialogueMixerBehaviour();

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (TimelineClip clip in GetClips())
            {
                if (clip.asset is DialogueAsset)
                {
                    DialogueAsset asset = clip.asset as DialogueAsset;
                    DialogueBehaviour behaviour = asset.BehaviourReference;

                    clip.displayName = asset.name;

                    behaviour.StartTime = clip.start;
                    behaviour.EndTime = clip.end;   
                }
            }

            return ScriptPlayable<DialogueMixerBehaviour>.Create(graph, Template, inputCount);
        }
    }
}
