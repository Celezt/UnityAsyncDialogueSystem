using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ActionEventAsset), true)]
    public class ActionEventAssetEditor : DSPlayableAssetEditor
    {
        public override void BuildInspector()
        {
            var asset = serializedObject.targetObject as ActionEventAsset;

            if (asset.Receiver == null)
                asset.Director.RebuildGraph();

            ActionReceiver reciver = asset.Receiver;
           
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
        }
    }
}
