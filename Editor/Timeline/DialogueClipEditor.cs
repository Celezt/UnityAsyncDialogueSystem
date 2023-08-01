using Celezt.DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem.Editor
{
    [CustomTimelineEditor(typeof(DialogueAsset))]
    public class DialogueClipEditor : ClipEditor
    {
        private readonly Color _offsetBackgroundColour = new Color(0.22f, 0.22f, 0.22f);

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as DialogueAsset;

            float startWidthOffset = region.position.width * (float)(asset.StartOffset / region.endTime);
            float endWidthOffset = region.position.width * (float)(asset.EndOffset / region.endTime);
            var startOffsetRegion = new Rect(region.position.position.x, region.position.position.y,
                                        startWidthOffset, region.position.height);
            var endOffsetRegion = new Rect(region.position.width - endWidthOffset, region.position.position.y,
                                        endWidthOffset, region.position.height);

            EditorGUI.DrawRect(startOffsetRegion, _offsetBackgroundColour);
            EditorGUI.DrawRect(endOffsetRegion, _offsetBackgroundColour);
        }
    }
}
