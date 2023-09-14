using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private UnityEngine.Object? _target;
        private UnityEngine.Object? _reference;
        private Type? _extensionType;
        private IExtension? _extension;

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
            _extension = property.managedReferenceValue as IExtension;

            if (_extension == null)
                return;

            _target = property.serializedObject.targetObject;
            var collection = _target as IExtensionCollection;
            _extensionType = _extension.GetType();
            long rid = property.managedReferenceId;

            if (!_isOpens.ContainsKey(rid))
                _isOpens.Add(rid, true);

            GUILayout.Space(-20);

            EditorGUILayoutExtra.DrawUILine(color: new Color(0.102f, 0.102f, 0.102f), padding: -3);

            var color = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1.1f, 1.1f, 1.1f);
            var content = new GUIContent(Extensions.Names[_extensionType]);
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
                var newReference = EditorGUI.ObjectField(referenceRect, GUIContent.none, _extension.Reference, typeof(ExtensionObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!ExtensionUtility.HasSelfReference(_target, newReference))
                    {
                        _extension.Reference = newReference;
                        EditorUtility.SetDirty(_target);
                        Undo.RecordObject(_target, "Changed Extension Reference");
                    }
                }
            }

            bool isOpen = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, _isOpens[rid], content, style, ShowHeaderContextMenu);
            EditorGUI.EndFoldoutHeaderGroup();

            // Need to draw it twice, the first for interaction and the second as overlay. Thanks Unity!
            if (collection != null)
                EditorGUI.ObjectField(referenceRect, GUIContent.none, _extension.Reference, typeof(ExtensionObject), false);

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

                        if (_extension.Reference != null)
                        {
                            using var serializedObject = new SerializedObject(_extension.Reference);
                            var serializedProperty = serializedObject.FindProperty(currentProperty.propertyPath);

                            ExtensionEditorUtility.DrawHasModification(currentProperty, serializedProperty);
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }

            _isOpens[rid] = isOpen;
            _reference = _extension.Reference;

            void ShowHeaderContextMenu(Rect rect)
            {
                var menu = new GenericMenu();
                if (collection != null)
                {
                    menu.AddItem(new GUIContent("Reset"), false, () =>
                    {
                        var newExtension = (IExtension)Activator.CreateInstance(_extensionType);
                        newExtension.Reference = _extension.Reference;
                        property.managedReferenceValue = newExtension;

                    });
                    menu.AddSeparator(null);
                    menu.AddItem(new GUIContent("Remove Extension"), false, () =>
                    {
                        collection.RemoveExtension(_extensionType);
                        EditorUtility.SetDirty(_target);
                        Undo.RecordObject(_target, "Removed Extension");

                    });
                }
                menu.DropDown(rect);
            }
        }

        void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            var serializedObject = property.serializedObject;

            // If it is the current focused object.
            if (_target == null || _target != serializedObject.targetObject)
                return;

            // Only add menu item if it is a property of the extension.
            string path = property.propertyPath;
            if (path.StartsWith("_extensions.Array.data["))
            {
                int endIndex = path.IndexOf(']', 23);
                bool containSubProperties = path.Skip(endIndex + 2).Any(x => x == '.');

                if (!containSubProperties)  // Skip if it is a sub property.
                {
                    if (_reference is IExtensionCollection otherCollection)
                    {
                        using var otherSerializedObject = new SerializedObject(_reference);
                        var otherProperty = otherSerializedObject.FindProperty(property.propertyPath);
                        IExtension otherExtension = otherCollection.Extensions[int.Parse(path.AsSpan(23, endIndex - 23))];

                        if (!SerializedProperty.DataEquals(property, otherProperty))
                        {
                            menu.AddItem(new GUIContent($"Apply to Reference '{_reference.name}'"), false, () =>
                            {

                            });
                            menu.AddItem(new GUIContent("Revert"), false, () =>
                            {
                                FieldInfo info = _extensionType!
                                    .GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                info.SetValue(_extension, info.GetValue(otherExtension));
                                EditorUtility.SetDirty(_target);
                                Undo.RecordObject(_target, "Revert");
                                serializedObject.Update();
                            });
                        }
                    }
                }
            }
        }
    }
}