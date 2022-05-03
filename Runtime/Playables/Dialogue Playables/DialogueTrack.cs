using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.2f, 0.4f, 0.5f)]
    [TrackClipType(typeof(DialogueAsset))]
    public class DialogueTrack : DSTrack
    {
        protected override DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, DialogueSystemBinder binder, PlayableDirector director, GameObject go, int inputCount)
        {
            var template = new DialogueMixerBehaviour();

            return template;
        }
    }
}
