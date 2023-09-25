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

        private SerializedProperty _textProperty;
        private SerializedProperty _startOffsetProperty;
        private SerializedProperty _endOffsetProperty;
        private SerializedProperty _editorVisibilityCurve;

        private string _runtimeText;

        protected override void OnDrawProperties(Rect position, SerializedProperty property, GUIContent label, IExtension extension)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var textExtension = extension as TextExtension;

            _textProperty ??= property.FindPropertyRelative("_editorText");
            _startOffsetProperty ??= property.FindPropertyRelative("_startOffset");
            _endOffsetProperty ??= property.FindPropertyRelative("_endOffset");
            _editorVisibilityCurve ??= property.FindPropertyRelative("_editorVisibilityCurve");

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Text" : "Text (Readonly)");

            EditorStyles.textField.wordWrap = true;
            
            if (EditorOrRuntime.IsEditor)
            {
                EditorGUI.BeginChangeCheck();
                _textProperty.stringValue = EditorGUILayout.TextArea(_textProperty.stringValue, GUILayout.MinHeight(150));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    textExtension.RefreshText();
                }
            }
            else
            {
                if (!System.MemoryExtensions.Equals(_runtimeText, textExtension.RuntimeText, System.StringComparison.Ordinal))
                    _runtimeText = textExtension.RuntimeText.ToString();

                using(EditorGUIExtra.Disable.Scope())
                    EditorGUILayout.TextArea(_runtimeText, GUILayout.MinHeight(150));
            }
            DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, _textProperty);

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Visibility Settings" : "Visibility Settings (Readonly)", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope())
                EditorGUILayout.Space(6);

            EditorGUI.indentLevel--;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(14, false);
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 10;

                using (EditorGUIExtra.Disable.Scope(EditorOrRuntime.IsRuntime))
                    textExtension.StartOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.StartOffset, GUILayout.Width(50));
                Rect modificationRect = GUILayoutUtility.GetLastRect();
                DrawHasModification(modificationRect, extension.Reference, _startOffsetProperty);

                if (EditorOrRuntime.IsEditor)
                {
                    EditorGUI.BeginChangeCheck();
                    var curve = EditorGUILayout.CurveField(textExtension.EditorVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Curve Modified");
                        textExtension.RuntimeVisibilityCurve.keys = curve.keys;
                        textExtension.UpdateTags();
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    EditorGUILayoutExtra.CurveField(textExtension.RuntimeVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    GUI.Box(GUILayoutUtility.GetLastRect(), _runtimeVisibilityCurveContent);
                }
                DrawHasModification(modificationRect, extension.Reference, _editorVisibilityCurve);
                GUI.Box(GUILayoutUtility.GetLastRect(), _editorVisibilityCurveContent);

                using (EditorGUIExtra.Disable.Scope(EditorOrRuntime.IsRuntime))
                    textExtension.EndOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.EndOffset, GUILayout.Width(50));
                DrawHasModification(modificationRect, extension.Reference, _endOffsetProperty);

                EditorGUIUtility.labelWidth = labelWidth;
            }
            EditorGUI.indentLevel++;
        }
    }
}
