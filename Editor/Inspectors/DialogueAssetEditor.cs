using Celezt.Timeline.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueAsset), true)]
    public class DialogueAssetEditor : PlayableAssetExtendedEditor
    {
        private static readonly string[] _toolbar = new string[] { "Editor", "Runtime" };

        public override void BuildInspector()
        {
            EditorGUI.indentLevel--;

            EditorOrRuntime.IsEditor = GUILayout.Toolbar(EditorOrRuntime.IsEditor ? 0 : 1, _toolbar) == 0 ? true : false;

            EditorGUILayout.Space(20);

            ExtensionEditorUtility.DrawExtensions(serializedObject, typeof(DialogueAsset));

            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
    }
}
