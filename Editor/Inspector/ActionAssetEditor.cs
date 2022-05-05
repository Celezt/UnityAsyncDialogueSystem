using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ActionEventAsset), true)]
    public class ActionAssetEditor : DSPlayableAssetEditor
    {
        public override void BuildInspector()
        {
            var asset = serializedObject.targetObject as DSPlayableAsset;
            var behaviour = asset.BehaviourReference as ActionEventBehaviour;

            if (behaviour.Receiver == null)
                behaviour.Director.RebuildGraph();

            ActionReceiver reciver = behaviour.Receiver;
           
            SerializedObject serializedBinder = new SerializedObject(reciver);

            int index = reciver.ActionBinderDictionary.Keys.IndexOf(asset);
            if (index != -1)
            {

                serializedBinder.Update();

                SerializedProperty actionBinderContainer = serializedBinder.FindProperty("_actionBinderDictionary.list");
                SerializedProperty actionBinder = actionBinderContainer.GetArrayElementAtIndex(index);

                EditorGUILayout.PropertyField(actionBinder.FindPropertyRelative("Value.OnEnter"));
                EditorGUILayout.PropertyField(actionBinder.FindPropertyRelative("Value.OnExit"));

                serializedBinder.ApplyModifiedProperties();
            }

            SerializeAllPropertyFields();
        }
    }
}
