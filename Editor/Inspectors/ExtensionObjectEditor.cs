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
            EditorGUILayout.Space(20);

            ExtensionEditorUtility.DrawExtensions(serializedObject, typeof(DialogueAsset));

            serializedObject.ApplyModifiedProperties();
            serializedObject.UpdateIfRequiredOrScript();
        }
    }
}
