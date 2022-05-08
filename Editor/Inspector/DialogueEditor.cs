using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(Dialogue))]
    public class DialogueEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            Dialogue dialogue = (Dialogue)target;

            //EditorGUILayout.LabelField("Dialogue Graph Version: " + dialogue.Content.DGVersion);

            DrawPropertiesExcluding(serializedObject, "m_Script");

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
