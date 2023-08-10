using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueAsset), true)]
    public class DialogueAssetEditor : DSPlayableAssetEditor
    {
        private static readonly GUIContent _visibilityOffsetContent = new GUIContent("s", "Visibility offset (seconds)");
        private static readonly GUIContent _visibilityCurveContent = new GUIContent("", "Visibility curve (x: time, y: visible)");

        public override void BuildInspector()
        {
            var asset = serializedObject.targetObject as DialogueAsset;

            if (!asset.IsReady)
                return;

            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Actor");
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(6);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_actor"), GUIContent.none);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_text"), GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                asset.RefreshDialogue();
            }

            EditorGUILayout.LabelField("Visibility Settings");
            using (new EditorGUILayout.VerticalScope())
                EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 10;
                asset.StartOffset = EditorGUILayout.FloatField(_visibilityOffsetContent, asset.StartOffset, GUILayout.Width(50));

                EditorGUI.BeginChangeCheck();
                var curve = EditorGUILayout.CurveField(asset.EditorVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));

                if (EditorGUI.EndChangeCheck())
                {
                    asset.RuntimeVisibilityCurve.keys = curve.keys;
                    asset.UpdateTags();
                    EditorUtility.SetDirty(asset);
                }

                GUI.Box(GUILayoutUtility.GetLastRect(), _visibilityCurveContent);
                asset.EndOffset = EditorGUILayout.FloatField(_visibilityOffsetContent, asset.EndOffset, GUILayout.Width(50));
            }

            EditorGUILayout.LabelField($"Text Info", new GUIStyle("PreMiniLabel"));
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"Characters: {asset.Length}", new GUIStyle("IN ThumbnailSelection"));
            }
            EditorGUILayout.CurveField(asset.RuntimeVisibilityCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
            EditorGUILayout.LabelField($"Current Frame", new GUIStyle("PreMiniLabel"));
            using (new EditorGUILayout.HorizontalScope())
            {
                float visibility = asset.VisibilityInterval;
                EditorGUILayout.LabelField($"Visible: {(visibility * 100).ToString("0.#")}%", new GUIStyle("IN ThumbnailSelection"));
                EditorGUILayout.LabelField($"Index: {asset.Index}", new GUIStyle("IN ThumbnailSelection"));
            }
        }
    }
}
