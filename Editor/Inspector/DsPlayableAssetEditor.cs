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
    public class DSPlayableAssetEditor : UnityEditor.Editor
    {
        public virtual void BuildInspector() { }
 
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();

            BuildInspector();         
            DrawPropertiesExcluding(serializedObject, "m_Script");

            if (GetType() == typeof(DSPlayableAssetEditor)) // Called if not derived.
                SerializeAllPropertyFields();

            serializedObject.ApplyModifiedProperties();
        }

        protected void SerializeAllPropertyFields(params string[] exceptions)
        {
            HashSet<string> exceptionHashSet = new HashSet<string>(exceptions);
            DSPlayableAsset asset = serializedObject.targetObject as DSPlayableAsset;
            DSPlayableBehaviour behaviour = asset.BehaviourReference;

            IEnumerable<SerializedProperty> serializedProperties =
                from s in behaviour.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => !exceptionHashSet.Contains(x.Name))
                    .Select(x => serializedObject.FindProperty("_template." + x.Name))
                where s != null select s;

            foreach (SerializedProperty serializedProperty in serializedProperties)
                EditorGUILayout.PropertyField(serializedProperty, true);
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
