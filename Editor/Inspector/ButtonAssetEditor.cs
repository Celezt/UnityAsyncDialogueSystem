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
            var asset = serializedObject.targetObject as ButtonAsset;
            var behaviour = asset.BehaviourReference as ButtonBehaviour;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonReference"));

            // Hide if no button is found.
            ExposedReference<ButtonBinder> button = (ExposedReference<ButtonBinder>)GetValue(asset, "ButtonReference");
            if (button.exposedName != null)
            {
                // Hide if no text mesh is found.
                dynamic textMesh = GetValue(behaviour, "_textMesh");
                if (textMesh.Value != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Text"));
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Condition"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Settings"));

            if (asset.Settings != null)
            {
                SerializedObject serializedSettings = new SerializedObject(asset.Settings);

                serializedSettings.Update();
                EditorGUI.indentLevel++;

                IEnumerable<SerializedProperty> serializedProperties =
                    from s in asset.Settings.GetType()
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
