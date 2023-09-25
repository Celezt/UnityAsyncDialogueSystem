using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem.Editor
{
    [CustomExtensionClipEditor(typeof(TextExtension))]
    public struct TextClipEditor : IExtensionClipEditor
    {
        private static readonly Color _offsetBackgroundColour = new Color(0f, 0f, 0f, 0.2f);
        private static readonly Color _timeSpeedCurveColour = new Color(0.7f, 0.9f, 1f, 0.2f);

        public int Order => 0;

        public void OnDrawBackground(TimelineClip clip, ClipBackgroundRegion region, IExtension extension)
        {
            var asset = extension as TextExtension;

            float length = (float)(clip.end - clip.start);
            float ratio = (float)(region.endTime - region.startTime) / length;
            float width = region.position.width / ratio;
            float startWidthOffset = width * (float)(asset.StartOffset / length);
            float endWidthOffset = width * (float)(asset.EndOffset / length);
            float existingWidth = width - startWidthOffset - endWidthOffset;
            var startOffsetRegion = new Rect(0, 0,
                                        startWidthOffset, region.position.height);
            var endOffsetRegion = new Rect(width - endWidthOffset, 0,
                                        endWidthOffset, region.position.height);
            var existingRegion = new Rect(0 + startWidthOffset, 0,
                                        existingWidth, region.position.height);

            EditorGUI.DrawRect(startOffsetRegion, _offsetBackgroundColour);
            EditorGUI.DrawRect(endOffsetRegion, _offsetBackgroundColour);
            EditorGUIExtra.DrawCurve(existingRegion, _timeSpeedCurveColour, asset.RuntimeVisibilityCurve);
        }
    }
}
