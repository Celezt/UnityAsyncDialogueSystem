using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(NodeAsset), true), CanEditMultipleObjects]
    public class NodeAssetEditor : UnityEditor.Editor
    {
        public virtual void BuildInspector() { }

        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            BuildInspector();
            DrawPropertiesExcluding(serializedObject, "m_Script");

            SerializedProperty inputArray = serializedObject.FindProperty("_inputs");
            SerializedProperty inputPortNumberArray = serializedObject.FindProperty("_inputPortNumbers");

            if (inputArray.arraySize > 0)
            {
                EditorGUILayout.LabelField("Inputs");
            }

            for (int i = 0; i < inputArray.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(inputPortNumberArray.GetArrayElementAtIndex(i), GUIContent.none);
                GUILayout.FlexibleSpace();
                EditorGUILayout.PropertyField(inputArray.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
               var asset = target as NodeAsset;
               asset.IsDirty();
            }
        }

        protected object GetValue(object instance, string name) => GetValue<object>(instance, name);
        protected T GetValue<T>(object instance, string name)
        {
            return (T)instance.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .GetValue(instance);
        }
    }
}
