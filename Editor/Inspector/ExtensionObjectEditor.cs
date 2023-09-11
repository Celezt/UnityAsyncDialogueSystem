using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(ExtensionObject), true)]
    public class ExtensionObjectEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var asset = serializedObject.targetObject as ExtensionObject;

            ExtensionEditorUtility.DrawExtensions(serializedObject);
        }
    }
}
