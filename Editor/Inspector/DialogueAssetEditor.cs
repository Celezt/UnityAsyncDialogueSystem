using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueAsset), true)]
    public class DialogueAssetEditor : DSPlayableAssetEditor
    {
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
            {
                EditorGUILayout.Space(6);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUIExtraUtility.TightLabel("s");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("<StartOffset>k__BackingField"), GUIContent.none, GUILayout.Width(50));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("<TimeSpeed>k__BackingField"), GUIContent.none);
                EditorGUIExtraUtility.TightLabel("s");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("<EndOffset>k__BackingField"), GUIContent.none, GUILayout.Width(50));
            }
        }
    }
}
