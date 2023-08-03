using Celezt.DialogueSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace Celezt.DialogueSystem.Editor
{
    [CustomTimelineEditor(typeof(DialogueAsset))]
    public class DialogueClipEditor : ClipEditor
    {
        private static readonly Color _offsetBackgroundColour = new Color(0f, 0f, 0f, 0.2f);
        private static readonly Color _timeSpeedCurveColour = new Color(0.7f, 0.9f, 1f, 0.4f);

        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            var asset = clip.asset as DialogueAsset;

            float length = (float)(clip.end - clip.start); 
            float ratio = (float)(region.endTime - region.startTime) / length;
            float width = region.position.width / ratio;
            float startWidthOffset = width * (float)(asset.StartOffset / length);
            float endWidthOffset = width * (float)(asset.EndOffset / length);
            float existingWidth = width - startWidthOffset - endWidthOffset;
            var startOffsetRegion = new Rect(0, 1,
                                        startWidthOffset, region.position.height);
            var endOffsetRegion = new Rect(width - endWidthOffset, 1,
                                        endWidthOffset, region.position.height);
            var existingRegion = new Rect(0 + startWidthOffset, 1,
                                        existingWidth, region.position.height);

            EditorGUI.DrawRect(startOffsetRegion, _offsetBackgroundColour);
            EditorGUI.DrawRect(endOffsetRegion, _offsetBackgroundColour);
            DisplayCurve(existingRegion, _timeSpeedCurveColour, asset.TimeSpeedCurve);
        }

        private static void DisplayCurve(Rect rect, Color color, AnimationCurve curve, int subdivitions = 10)
        {
            if (curve.length < 1)
                return;

            int iterationCount = (int)(Mathf.Log10(rect.width) * subdivitions);
            float previousTime = 0;
            float previousValue = 0;

            Handles.color = color;
            for (int i = 0; i < iterationCount + 1; i++)
            {
                float time = i / (float)iterationCount;
                float value = curve.Evaluate(time);

                Handles.DrawAAPolyLine(3,
                    new Vector3(previousTime * rect.width + rect.position.x, (1 - previousValue) * rect.height, 0),
                    new Vector3(time * rect.width + rect.position.x, (1 - value) * rect.height, 0));

                previousTime = time;
                previousValue = value;
            }
        }
    }
}
