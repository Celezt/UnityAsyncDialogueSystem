using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DSPlayableAsset<>), true), CanEditMultipleObjects]
    public class DialogueAssetEditor : UnityEditor.Editor
    {
        public virtual void BuildInspector() { }
 
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializeAllPropertyField();
            BuildInspector();         
            DrawPropertiesExcluding(serializedObject, "m_Script");

            serializedObject.ApplyModifiedProperties();
        }

        private void SerializeAllPropertyField()
        {
            DSPlayableAsset asset = serializedObject.targetObject as DSPlayableAsset;
            DSPlayableBehaviour behaviour = asset.BehaviourReference;

            IEnumerable<SerializedProperty> serializedProperties =
                from s in behaviour.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(x => serializedObject.FindProperty("_template." + x.Name))
                where s != null select s;

            foreach (SerializedProperty serializedProperty in serializedProperties)
                EditorGUILayout.PropertyField(serializedProperty, true);
        }
    }
}
