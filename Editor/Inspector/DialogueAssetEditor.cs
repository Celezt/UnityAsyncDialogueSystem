using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueAsset), true)]
    public class DialogueAssetEditor : DSPlayableAssetEditor
    {
        private static readonly GUIContent _timeSpeedContent = new GUIContent("s", "Time Offset");

        public override void BuildInspector()
        {
            var asset = serializedObject.targetObject as DialogueAsset;

            EditorGUI.indentLevel--;
            EditorGUILayout.LabelField("Actor");
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.Space(6);
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_actor"), GUIContent.none);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_text"), GUIContent.none);
            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
                asset.UpdateTrimmedText();

            EditorGUILayout.LabelField("Speed Settings");
            using (new EditorGUILayout.VerticalScope())
                EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIUtility.labelWidth = 10;
                asset.StartOffset = EditorGUILayout.FloatField(_timeSpeedContent, asset.StartOffset, GUILayout.Width(50));
                asset.TimeSpeedCurve = EditorGUILayout.CurveField(asset.TimeSpeedCurve, new Color(0.4f, 0.6f, 0.7f), new Rect(0, 0, 1, 1));
                asset.EndOffset = EditorGUILayout.FloatField(_timeSpeedContent, asset.EndOffset, GUILayout.Width(50));
            }
        }
    }
}
