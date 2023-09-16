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

            foreach (var extension in asset.Extensions)
                extension.OnCreate(graph, go, clip);

            //            if (!asset.RuntimeText.IsEmpty)
            //                clip.displayName = Tags.TrimTextTags(asset.RuntimeText.ReadOnlySpan);

            //#if UNITY_EDITOR
            //            if (asset.HasUpdated)
            //                asset.HasUpdated = false;
            //            else
            //#endif
            //                asset.UpdateTags();
        }
    }
}
