using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
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

            if (!asset.RuntimeText.IsEmpty)
                clip.displayName = Tags.TrimTextTags(asset.RuntimeText.ToString());

#if UNITY_EDITOR
            if (asset.HasUpdated)
                asset.HasUpdated = false;
            else
#endif
                asset.UpdateTags();
        }
    }
}
