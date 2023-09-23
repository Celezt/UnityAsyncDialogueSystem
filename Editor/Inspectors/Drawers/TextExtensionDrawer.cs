using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(TextExtension), true)]
    public class TextExtensionDrawer : ExtensionDrawer
    {
        private static readonly GUIContent _editorVisibilityOffsetContent = new("s", "Editor visibility offset (seconds)");
        private static readonly GUIContent _editorVisibilityCurveContent = new("", "Editor visibility curve (x: time, y: visible)");
        private static readonly GUIContent _runtimeVisibilityCurveContent = new("", "Runtime visibility curve (x: time, y: visible)");

        private static readonly Color _offsetBackgroundColour = new Color(0f, 0f, 0f, 0.2f);
        private static readonly Color _timeSpeedCurveColour = new Color(0.7f, 0.9f, 1f, 0.2f);

        private string _runtimeText;

        protected override void OnDrawBackground(TimelineClip clip, ClipBackgroundRegion region, IExtension extension)
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

        protected override void OnDrawProperties(Rect position, SerializedProperty property, GUIContent label, IExtension extension)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var textExtension = extension as TextExtension;

            if (EditorOrRuntime.IsEditor)
                EditorContent();
            else
                RuntimeContent();

            void EditorContent()
            {
                EditorGUILayout.LabelField("Text");
                EditorStyles.textField.wordWrap = true;
                EditorGUI.BeginChangeCheck();
                var textProperty = property.FindPropertyRelative("_editorText");
                textProperty.stringValue = EditorGUILayout.TextArea(textProperty.stringValue, GUILayout.MinHeight(150));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    textExtension.RefreshText();
                }
                DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, textProperty);

                EditorGUILayout.LabelField("Visibility Settings", EditorStyles.boldLabel);
                using (new EditorGUILayout.VerticalScope())
                    EditorGUILayout.Space(6);

                EditorGUI.indentLevel--;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space(14, false);
                    float labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 10;
                    textExtension.StartOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.StartOffset, GUILayout.Width(50));
                    var startOffsetProperty = property.FindPropertyRelative("_startOffset");
                    DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, startOffsetProperty);

                    EditorGUI.BeginChangeCheck();
                    var curve = EditorGUILayout.CurveField(textExtension.EditorVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Curve Modified");
                        textExtension.RuntimeVisibilityCurve.keys = curve.keys;
                        textExtension.UpdateTags();
                        EditorUtility.SetDirty(target);
                    }
                    var editorVisibilityCurve = property.FindPropertyRelative("_editorVisibilityCurve");
                    DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, editorVisibilityCurve);

                    GUI.Box(GUILayoutUtility.GetLastRect(), _editorVisibilityCurveContent);
                    textExtension.EndOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.EndOffset, GUILayout.Width(50));
                    var endOffsetProperty = property.FindPropertyRelative("_endOffset");
                    DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, endOffsetProperty);

                    EditorGUIUtility.labelWidth = labelWidth;
                }
                EditorGUI.indentLevel++;
            }

            void RuntimeContent()
            {
                if (_runtimeText == null)
                    _runtimeText = textExtension.RuntimeText.ToString();

                EditorGUILayout.LabelField("Text (Readonly)");
                EditorStyles.textField.wordWrap = true;
                GUI.enabled = false;
                EditorGUILayout.TextArea(_runtimeText, GUILayout.MinHeight(150));
                GUI.enabled = true;

                EditorGUILayout.LabelField("Visibility Settings (Readonly)", EditorStyles.boldLabel);
                using (new EditorGUILayout.VerticalScope())
                    EditorGUILayout.Space(6);

                EditorGUI.indentLevel--;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space(14, false);
                    float labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 10;
                    GUI.enabled = false;
                    EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.StartOffset, GUILayout.Width(50));
                    GUI.enabled = true;

                    EditorGUILayoutExtra.CurveField(textExtension.RuntimeVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    GUI.Box(GUILayoutUtility.GetLastRect(), _runtimeVisibilityCurveContent);

                    GUI.enabled = false;
                    EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.EndOffset, GUILayout.Width(50));
                    GUI.enabled = true;
                    EditorGUIUtility.labelWidth = labelWidth;
                }
                EditorGUI.indentLevel++;
            }
        }
    }
}
