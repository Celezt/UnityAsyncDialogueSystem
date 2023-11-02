using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ExtensionObject), true)]
    public class ExtensionObjectEditor : UnityEditor.Editor
    {
        private static readonly string[] _toolbar = new string[] { "Editor", "Runtime" };

        public override void OnInspectorGUI()
        {
            EditorOrRuntime.IsEditor = GUILayout.Toolbar(EditorOrRuntime.IsEditor ? 0 : 1, _toolbar) == 0 ? true : false;

            EditorGUILayout.Space(20);

            ExtensionEditorUtility.DrawExtensions(serializedObject, typeof(DialogueAsset));

            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
    }
}
