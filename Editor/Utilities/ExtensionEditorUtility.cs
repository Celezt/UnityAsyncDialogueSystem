using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem.Editor
{
    internal static class ExtensionEditorUtility
    {
        public static string[] ExtensionOptions
        {
            get
            {
                if (_extensionOptions == null)
                {
                    IEnumerable<string> extensions = Extensions.Types.Keys;
                    _extensionOptions = new string[extensions.Count() + 1];
                    _extensionOptions[0] = "(Select)";

                    int count = 1;
                    foreach (string extension in extensions)
                        _extensionOptions[count++] = extension;
                }

                return _extensionOptions;
            }
        }

        private static string[]? _extensionOptions;

        public static void DrawExtensions(SerializedObject serializedObject)
        {
            var target = serializedObject.targetObject;
            var asset = target as IExtensionCollection;

            if (asset == null)
                return;

            EditorGUILayout.LabelField("Extensions", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Add Extension");

                EditorGUI.BeginChangeCheck();
                int index = EditorGUILayout.Popup(0, ExtensionOptions);
                if (EditorGUI.EndChangeCheck())
                {
                    Type type = Extensions.Types[ExtensionOptions[index]];

                    Undo.RecordObject(target, $"Added Extension '{Extensions.Names[type]}'");
                    IExtension extension = (IExtension)Activator.CreateInstance(type);
                    extension.Target = target;
                    asset.AddExtension(extension);
                    EditorUtility.SetDirty(target);
                    serializedObject.Update();
                }
            }
            GUILayout.Space(8);

            var extensionsProperty = serializedObject.FindProperty("_extensions");

            IEnumerator enumerator = extensionsProperty.GetEnumerator();
            int depth = extensionsProperty.depth;

            while (enumerator.MoveNext())
            {
                var currentExtensionProperty = enumerator.Current as SerializedProperty;

                if (currentExtensionProperty == null || currentExtensionProperty.depth > depth + 1)
                    continue;

                EditorGUILayout.PropertyField(currentExtensionProperty, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static bool DrawHasModification(SerializedProperty lhs, SerializedProperty rhs)
        {
            // If content in current property is not the same as reference property. 
            if (!SerializedProperty.DataEquals(lhs, rhs))
            {
                Rect rect = GUILayoutUtility.GetLastRect();
                rect.x = 0;
                rect.width = 2;
                EditorGUI.DrawRect(rect, new Color(0.06f, 0.50f, 0.75f));
                return true;
            }

            return false;
        }
    }
}
