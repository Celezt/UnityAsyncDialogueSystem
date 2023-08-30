using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueAsset), true)]
    public class DialogueAssetEditor : DSPlayableAssetEditor
    {
        private static readonly GUIContent _editorVisibilityOffsetContent = new("s", "Editor Visibility offset (seconds)");
        private static readonly GUIContent _editorVisibilityCurveContent = new("", "Editor Visibility curve (x: time, y: visible)");
        private static readonly GUIContent _runtimeVisibilityCurveContent = new("", "Runtime Visibility curve (x: time, y: visible)");

        private static readonly string[] _toolbar = new string[] { "Editor", "Runtime" };

        private int _toolbarIndex;
        private string _runtimeText;

        public override void BuildInspector()
        {
            var asset = serializedObject.targetObject as DialogueAsset;

            if (!asset.IsReady)
                return;

            GUIStyle infoStyle = new GUIStyle("IN ThumbnailSelection");
            GUIStyle infoTitleStyle = new GUIStyle("PreMiniLabel");

            EditorGUI.indentLevel--;

            _toolbarIndex = GUILayout.Toolbar(_toolbarIndex, _toolbar);

            switch(_toolbarIndex)
            {
                case 0:
                    EditorContent();
                    break;
                case 1:
                    RuntimeContent(); 
                    break;
            }

            EditorGUILayout.LabelField("Visibility Settings");
            using (new EditorGUILayout.VerticalScope())
                EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 10;
                asset.StartOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, asset.StartOffset, GUILayout.Width(50));

                EditorGUI.BeginChangeCheck();
                var curve = EditorGUILayout.CurveField(asset.EditorVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));

                if (EditorGUI.EndChangeCheck())
                {
                    asset.RuntimeVisibilityCurve.keys = curve.keys;
                    asset.UpdateTags();
                    EditorUtility.SetDirty(asset);
                }

                GUI.Box(GUILayoutUtility.GetLastRect(), _editorVisibilityCurveContent);
                asset.EndOffset = EditorGUILayout.FloatField(_editorVisibilityOffsetContent, asset.EndOffset, GUILayout.Width(50));
            }

            EditorGUILayout.LabelField($"Text Info", infoTitleStyle);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Characters: {asset.Length}", infoStyle);
            }

            EditorGUILayoutExtra.CurveField(asset.RuntimeVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
            GUI.Box(GUILayoutUtility.GetLastRect(), _runtimeVisibilityCurveContent);
            EditorGUILayout.LabelField($"Current Frame", infoTitleStyle);

            using (new EditorGUILayout.HorizontalScope())
            {
                float visibility = asset.VisibilityInterval;
                EditorGUILayout.LabelField($"Visible: {(visibility * 100).ToString("0.#")}%", infoStyle);
                EditorGUILayout.LabelField($"Tan: {asset.Tangent.ToString("0.##")}", infoStyle);
                EditorGUILayout.LabelField($"Index: {asset.Index}", infoStyle);
            }

            void EditorContent()
            {
                EditorGUILayout.LabelField("Actor");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("_actor"), GUIContent.none);

                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Text");
                EditorStyles.textField.wordWrap = true;
                EditorGUI.BeginChangeCheck();
                var textProperty = serializedObject.FindProperty("_editorText");
                textProperty.stringValue = EditorGUILayout.TextField(textProperty.stringValue, GUILayout.MinHeight(150));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    asset.RefreshDialogue();
                }
            }

            void RuntimeContent()
            {
                if (_runtimeText == null)
                    _runtimeText = asset.RuntimeText.ToString();

                EditorGUILayout.LabelField("Actor (Readonly)");

                EditorGUILayout.LabelField(asset.Actor, EditorStyles.textField);

                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Text (Readonly)");
                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.LabelField(_runtimeText, EditorStyles.textField, GUILayout.MinHeight(150));
            }
        }
    }
}
