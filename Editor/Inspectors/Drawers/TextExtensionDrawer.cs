using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using static UnityEngine.GraphicsBuffer;

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(TextExtension), true)]
    public class TextExtensionDrawer : ExtensionDrawer
    {
        private static readonly GUIContent _editorVisibilityOffsetContent = new("s", "Editor visibility offset (seconds)");
        private static readonly GUIContent _editorVisibilityCurveContent = new("", "Editor visibility curve (x: time, y: visible)");
        private static readonly GUIContent _runtimeVisibilityCurveContent = new("", "Runtime visibility curve (x: time, y: visible)");

        private string _runtimeText;

        protected override void OnDrawProperties(Rect position, SerializedProperty property, GUIContent label, IExtension extension)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var textExtension = extension as TextExtension;

            SerializedProperty textProperty = property.FindPropertyRelative("_editorText");
            SerializedProperty startOffsetProperty = property.FindPropertyRelative("_startOffset");
            SerializedProperty endOffsetProperty = property.FindPropertyRelative("_endOffset");
            SerializedProperty editorVisibilityCurve = property.FindPropertyRelative("_editorVisibilityCurve");

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Text" : "Text (Readonly)");

            EditorStyles.textField.wordWrap = true;
            
            if (EditorOrRuntime.IsEditor)
            {
                EditorGUI.BeginChangeCheck();
                string text = EditorGUILayout.TextArea(textProperty.stringValue, GUILayout.MinHeight(150));
                if (EditorGUI.EndChangeCheck())
                {
                    textProperty.stringValue = text;
                    serializedObject.ApplyModifiedProperties();
                    extension.SetModified(textProperty.name, true);
                    textExtension.RefreshText();
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                if (!System.MemoryExtensions.Equals(_runtimeText, textExtension.RuntimeText, System.StringComparison.Ordinal))
                    _runtimeText = textExtension.RuntimeText.ToString();

                using(EditorGUIExtra.Disable.Scope())
                    EditorGUILayout.TextArea(_runtimeText, GUILayout.MinHeight(150));
            }
            DrawModification(GUILayoutUtility.GetLastRect(), textProperty, extension);

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Visibility Settings" : "Visibility Settings (Readonly)", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope())
                EditorGUILayout.Space(6);

            EditorGUI.indentLevel--;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.Space(14, false);
                float labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 10;

                ExtensionEditorUtility.FloatField(_editorVisibilityOffsetContent, extension, startOffsetProperty, GUILayout.Width(50));
                Rect modificationRect = GUILayoutUtility.GetLastRect();
                DrawModification(modificationRect, startOffsetProperty, extension);

                if (EditorOrRuntime.IsEditor)
                {
                    EditorGUI.BeginChangeCheck();
                    var curve = EditorGUILayout.CurveField(textExtension.EditorVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Curve Modified");
                        textExtension.RuntimeVisibilityCurve.keys = curve.keys;
                        extension.SetModified(editorVisibilityCurve.name, true);
                        textExtension.UpdateTags();
                        EditorUtility.SetDirty(target);
                    }
                }
                else
                {
                    EditorGUILayoutExtra.CurveField(textExtension.RuntimeVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    GUI.Box(GUILayoutUtility.GetLastRect(), _runtimeVisibilityCurveContent);
                }
                DrawModification(modificationRect, editorVisibilityCurve, extension);
                GUI.Box(GUILayoutUtility.GetLastRect(), _editorVisibilityCurveContent);

                ExtensionEditorUtility.FloatField(_editorVisibilityOffsetContent, extension, endOffsetProperty, GUILayout.Width(50));
                DrawModification(modificationRect, endOffsetProperty, extension);

                EditorGUIUtility.labelWidth = labelWidth;
            }
            EditorGUI.indentLevel++;
        }
    }
}
