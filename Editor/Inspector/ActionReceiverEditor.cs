using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ActionReceiver))]
    public class ActionReceiverEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}
