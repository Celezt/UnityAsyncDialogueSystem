using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(TextExtension), true)]
    public class TextExtensionDrawer : ExtensionDrawer
    {
        private static readonly GUIContent _editorVisibilityOffsetContent = new("s", "Editor visibility offset (seconds)");
        private static readonly GUIContent _editorVisibilityCurveContent = new("", "Editor visibility curve (x: time, y: visible)");
        private static readonly GUIContent _runtimeVisibilityCurveContent = new("", "Runtime visibility curve (x: time, y: visible)");

        private string _runtimeText;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, IExtension extension)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var textExtension = extension as TextExtension;

            if (EditorOrRuntime.IsEditor)
                EditorContent();
            else
                RuntimeContent();

            EditorGUILayout.Space(10);

            void EditorContent()
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Text");
                EditorStyles.textField.wordWrap = true;
                EditorGUI.BeginChangeCheck();
                var textProperty = serializedObject.FindProperty("_editorText");
                textProperty.stringValue = EditorGUILayout.TextArea(textProperty.stringValue, GUILayout.MinHeight(150));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    textExtension.RefreshDialogue();
                }

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

                    EditorGUI.BeginChangeCheck();
                    var curve = EditorGUILayout.CurveField(textExtension.EditorVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Curve Modified");
                        textExtension.RuntimeVisibilityCurve.keys = curve.keys;
                        textExtension.UpdateTags();
                        EditorUtility.SetDirty(target);
                    }

                    GUI.Box(GUILayoutUtility.GetLastRect(), _editorVisibilityCurveContent);
                    textExtension.EndOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, textExtension.EndOffset, GUILayout.Width(50));
                    EditorGUIUtility.labelWidth = labelWidth;
                }
                EditorGUI.indentLevel++;
            }

            void RuntimeContent()
            {
                if (_runtimeText == null)
                    _runtimeText = textExtension.RuntimeText.ToString();

                EditorGUILayout.Space(8);
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
