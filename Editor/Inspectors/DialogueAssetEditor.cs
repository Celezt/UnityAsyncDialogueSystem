using Celezt.Timeline.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DialogueAsset), true)]
    public class DialogueAssetEditor : PlayableAssetExtendedEditor
    {
        private static readonly string[] _toolbar = new string[] { "Editor", "Runtime" };

        private int _toolbarIndex;

        public override void BuildInspector()
        {
            var asset = serializedObject.targetObject as DialogueAsset;

            if (!asset.IsReady)
                return;

            GUIStyle infoStyle = new GUIStyle("IN ThumbnailSelection");
            GUIStyle infoTitleStyle = new GUIStyle("PreMiniLabel");

            EditorGUI.indentLevel--;

            _toolbarIndex = GUILayout.Toolbar(_toolbarIndex, _toolbar);

            EditorGUILayout.Space(20);

            if (_toolbarIndex == 0)
                EditorOrRuntime.IsEditor = true;
            else if (_toolbarIndex == 1)
                EditorOrRuntime.IsRuntime = true;

            ExtensionEditorUtility.DrawExtensions(serializedObject, typeof(DialogueAsset));

            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
    }
}
