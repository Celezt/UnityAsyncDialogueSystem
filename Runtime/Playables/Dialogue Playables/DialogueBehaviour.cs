using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class DialogueBehaviour : DSPlayableBehaviour
    {
        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            DialogueAsset asset = Asset as DialogueAsset;

            if (!string.IsNullOrWhiteSpace(asset.RawText))
                clip.displayName = Tags.TrimTextTags(asset.Text);

            asset.UpdateTags();
        }
    }
}
