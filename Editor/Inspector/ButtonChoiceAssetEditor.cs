using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ButtonChoiceAsset))]
    public class ButtonChoiceAssetEditor : DSPlayableAssetEditor
    {
        public override void BuildInspector()
        {
            DSPlayableAsset asset = serializedObject.targetObject as DSPlayableAsset;
            ButtonChoiceBehaviour behaviour = asset.BehaviourReference as ButtonChoiceBehaviour;

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
