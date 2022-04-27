using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Celezt.DialogueSystem
{
    public class DialogueAssetEditor : UnityEditor.Editor
    {
        private Dictionary<string, SerializedProperty> serializedProperties = new Dictionary<string, SerializedProperty>();

        public virtual void BuildInspector() { }

        protected void AddPropertyField(string propertyName)
        {
            if (serializedProperties.ContainsKey(propertyName))
                return;

            SerializedProperty property = serializedObject.FindProperty("Template." + propertyName);
            serializedProperties.Add(propertyName, property);
            
            EditorGUILayout.PropertyField(property);
        }
 
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();

            BuildInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
