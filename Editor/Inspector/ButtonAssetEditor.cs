using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ButtonAsset))]
    public class ButtonAssetEditor : DSPlayableAssetEditor
    {
        public override void BuildInspector()
        {
            DSPlayableAsset asset = serializedObject.targetObject as DSPlayableAsset;
            ButtonBehaviour behaviour = asset.BehaviourReference as ButtonBehaviour;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_template.ButtonReference"));

            // Hide if no button is found.
            object button = GetValue(behaviour, "_button");
            if (button != null)
            {
                // Hide if no text mesh is found.
                object textMesh = GetValue(behaviour, "_textMesh");
                if (textMesh != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("_template.Text"));
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_template.Condition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_template.Settings"));

            if (behaviour.Settings != null)
            {
                SerializedObject serializedSettings = new SerializedObject(behaviour.Settings);

                serializedSettings.Update();
                EditorGUI.indentLevel++;

                IEnumerable<SerializedProperty> serializedProperties =
                    from s in behaviour.Settings.GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Select(x => serializedSettings.FindProperty(x.Name))
                    where s != null
                    select s;

                foreach (SerializedProperty serializedProperty in serializedProperties)
                    EditorGUILayout.PropertyField(serializedProperty, true);

                EditorGUI.indentLevel--;
                serializedSettings.ApplyModifiedProperties();
            }
        }
    }
}
