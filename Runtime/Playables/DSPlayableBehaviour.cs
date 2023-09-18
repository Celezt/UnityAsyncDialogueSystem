using Celezt.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class DSPlayableBehaviour : PlayableBehaviourExtended
    {
        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            DSPlayableAsset asset = Asset as DSPlayableAsset;

            foreach (var extension in asset.Extensions)
                extension.OnCreate(graph, go, clip);
        }
    }
}
