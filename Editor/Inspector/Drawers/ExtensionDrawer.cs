using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(Extension), true)]
    public class ExtensionDrawer : PropertyDrawer
    {
        private static Dictionary<long, bool> _isOpens = new();

        private long _currentRid;

        public ExtensionDrawer()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        }

        ~ExtensionDrawer()
        {
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var extension = property.managedReferenceValue as IExtension;

            if (extension == null)
                return;

            var target = property.serializedObject.targetObject;
            var collection = target as IExtensionCollection;
            Type extensionType = extension.GetType();
            _currentRid = property.managedReferenceId;

            if (!_isOpens.ContainsKey(_currentRid))
                _isOpens.Add(_currentRid, true);

            GUILayout.Space(-20);

            EditorGUILayoutExtra.DrawUILine(color: new Color(0.102f, 0.102f, 0.102f), padding: -3);

            var color = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1.1f, 1.1f, 1.1f);
            var content = new GUIContent(Extensions.Names[extensionType]);
            Rect foldoutRect = GUILayoutUtility.GetRect(content, EditorStyles.foldoutHeader);
            foldoutRect.x += 14.0f;
            foldoutRect.width -= 14.0f;
            var style = EditorStyles.foldoutHeader;
            style.fixedHeight = 20;

            float referencePadding = 20;
            float referenceWidth = 140;
            Rect referenceRect = new Rect(foldoutRect.x + foldoutRect.width - referenceWidth - referencePadding,
                                            foldoutRect.y + 1, referenceWidth, foldoutRect.height - 2);

            if (foldoutRect.width < referenceRect.width + referencePadding)
            {
                referenceRect.x = foldoutRect.x;
                referenceRect.width = foldoutRect.width - referencePadding;
            }

            if (collection != null)
            {
                EditorGUI.BeginChangeCheck();
                var newReference = EditorGUI.ObjectField(referenceRect, GUIContent.none, extension.Reference, typeof(ExtensionObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!ExtensionUtility.HasSelfReference(target, newReference))
                        extension.Reference = newReference;
                }
            }

            bool isOpen = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, _isOpens[_currentRid], content, style, ShowHeaderContextMenu);
            EditorGUI.EndFoldoutHeaderGroup();

            // Need to draw it twice, the first for interaction and the second as overlay. Thanks Unity!
            if (collection != null)
                EditorGUI.ObjectField(referenceRect, GUIContent.none, extension.Reference, typeof(ExtensionObject), false);

            GUI.backgroundColor = color;

            EditorGUILayoutExtra.DrawUILine(
                color: isOpen ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.102f, 0.102f, 0.102f),
                padding: -4);

            if (isOpen)
            {
                EditorGUI.indentLevel++;
                if (GetType() == typeof(ExtensionDrawer))             // If not inherited.
                {
                    var iteratorProperty = property.Copy();
                    IEnumerator enumerator = iteratorProperty.GetEnumerator();
                    int depth = property.depth;

                    while (enumerator.MoveNext())
                    {
                        var currentProperty = enumerator.Current as SerializedProperty;

                        if (currentProperty == null || currentProperty.depth > depth + 1)
                            continue;

                        EditorGUILayout.PropertyField(currentProperty, true);

                        if (extension.Reference != null)
                        {
                            using var serializedObject = new SerializedObject(extension.Reference);
                            var serializedProperty = serializedObject.FindProperty(currentProperty.propertyPath);

                            ExtensionEditorUtility.DrawHasModification(currentProperty, serializedProperty);
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            _isOpens[_currentRid] = isOpen;

            void ShowHeaderContextMenu(Rect rect)
            {
                var menu = new GenericMenu();
                if (collection != null)
                {
                    menu.AddItem(new GUIContent("Reset"), false, () =>
                    {
                        var newExtension = (IExtension)Activator.CreateInstance(extensionType);
                        newExtension.Reference = extension.Reference;
                        property.managedReferenceValue = newExtension;
                       
                    });
                    menu.AddSeparator(null);
                    menu.AddItem(new GUIContent("Remove Extension"), false, () =>
                    {
                        collection.RemoveExtension(extensionType);
                        EditorUtility.SetDirty(target);
                        Undo.RecordObject(target, "Removed Extension");

                    });
                }
                menu.DropDown(rect);
            }
        }

        void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            // Only add menu item if it is a property of the extension.
            string path = property.propertyPath;
            if (path.StartsWith("_extensions.Array.data["))
            {
                int endIndex = path.IndexOf(']', 23);
                bool containSubProperties = path.Skip(endIndex + 2).Any(x => x == '.');

                if (!containSubProperties)  // Skip if it is a sub property.
                {
                    int arrayIndex = int.Parse(path.AsSpan(23, endIndex - 23));

                    var parentProperty = property.serializedObject.FindProperty($"_extensions.Array.data[{arrayIndex}]");

                    // If it is the current focused extension.
                    if (parentProperty.managedReferenceId == _currentRid)
                    {
                        menu.AddItem(new GUIContent("Reset"), false, () =>
                        {

                        });
                    }
                }
            }
        }
    }
}