using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ActionEventAsset), true)]
    public class ActionAssetEditor : DsPlayableAssetEditor
    {
        public override void BuildInspector()
        {
            DSPlayableAsset asset = serializedObject.targetObject as DSPlayableAsset;

            if (asset.BehaviourReference.Binder == null)
                asset.BehaviourReference.Director.RebuildGraph();

            DialogueSystemBinder binder = asset.BehaviourReference.Binder;
           
            SerializedObject serializedBinder = new SerializedObject(binder);

            int index = binder.ActionBinderDictionary.Keys.IndexOf(asset);
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
