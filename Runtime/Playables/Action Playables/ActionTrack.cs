using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [TrackColor(0.7f, 0.2f, 0.2f)]
    [TrackClipType(typeof(ActionEventAsset))]
    [TrackClipType(typeof(ButtonChoiceAsset))]
    public class ActionTrack : DSTrack
    {
        protected override DSMixerBehaviour CreateTrackMixer(PlayableGraph graph, DialogueSystemBinder binder, PlayableDirector director, GameObject go, int inputCount)
        {
            var template = new ActionMixerBehaviour();

            return template;
        }
    }
}
