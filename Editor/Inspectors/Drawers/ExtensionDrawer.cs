using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UIElements;

#nullable enable

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(IExtension), true)]
    public class ExtensionDrawer : PropertyDrawer
    {
        public virtual int Order => 0;

        private static Dictionary<long, bool> _isOpens = new();

        private UnityEngine.Object? _target;
        private UnityEngine.Object? _reference;
        private IExtension? _extension;

        private GenericMenu? _propertyContextMenu;
        private Rect _propertyContextMenuRect;
        private List<(Rect Rect, SerializedProperty Property)> _contextMenuProperties = new();

        public ExtensionDrawer()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        }

        ~ExtensionDrawer()
        {
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        }

        protected void DrawHasModification(Rect rect, UnityEngine.Object? reference, SerializedProperty property, bool clickable = true)
        {
            if (reference == null)
                return;

            using var referenceSerializedObject = new SerializedObject(reference);
            var referenceSerializedProperty = referenceSerializedObject.FindProperty(property.propertyPath);

            rect.x = 0;
            rect.width = 20;

            if (clickable)
                AddPropertyToContextMenu(rect, property);

            ExtensionEditorUtility.DrawHasModification(rect, property, referenceSerializedProperty);
        }

        protected void AddPropertyToContextMenu(Rect rect, SerializedProperty property)
        {
            var defaultColor = GUI.color;
            GUI.color = Color.clear;
            if (GUI.Button(rect, GUIContent.none, GUI.skin.box))
            {
                if (Event.current.button == 1)
                {
                    _propertyContextMenuRect = rect;
                    _propertyContextMenu ??= new GenericMenu();
                }
            }
            GUI.color = defaultColor;

            _contextMenuProperties.Add((rect, property));
        }

        protected virtual void OnDrawBackground(TimelineClip clip, ClipBackgroundRegion region, IExtension extension) { }

        protected virtual void OnDrawProperties(Rect position, SerializedProperty property, GUIContent label, IExtension extension) 
        {
            var iteratorProperty = property.Copy();
            IEnumerator enumerator = iteratorProperty.GetEnumerator();
            int depth = property.depth;

            if (EditorOrRuntime.IsRuntime)
                GUI.enabled = false;

            while (enumerator.MoveNext())
            {
                var currentProperty = enumerator.Current as SerializedProperty;

                if (currentProperty == null || currentProperty.depth > depth + 1)
                    continue;

                EditorGUILayout.PropertyField(currentProperty, true);

                if (extension.Reference != null)
                {
                    using var referenceSerializedObject = new SerializedObject(extension.Reference);
                    var referenceSerializedProperty = referenceSerializedObject.FindProperty(currentProperty.propertyPath);

                    ExtensionEditorUtility.DrawHasModification(GUILayoutUtility.GetLastRect(), currentProperty, referenceSerializedProperty);
                }
            }

            GUI.enabled = true;
        }

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _extension = property.managedReferenceValue as IExtension;

            if (_extension == null)
                return;

            var serializedObject = property.serializedObject;
            _target = serializedObject.targetObject;
            var collection = _target as IExtensionCollection;
            Type extensionType = _extension.GetType();
            long rid = property.managedReferenceId;

            if (!_isOpens.ContainsKey(rid))
                _isOpens.Add(rid, true);

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
                var newReference = EditorGUI.ObjectField(referenceRect, GUIContent.none, _extension.Reference, typeof(ExtensionObject), false);
                if (EditorGUI.EndChangeCheck())
                {
                    if (!ExtensionUtility.HasSelfReference(_target, newReference))
                    {
                        Undo.RecordObject(_target, "Changed Extension Reference");
                        _extension.Reference = newReference;
                        EditorUtility.SetDirty(_target);
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
                EditorGUILayout.Space(4);
                EditorGUI.indentLevel++;

                OnDrawProperties(position, property, label, _extension);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(10);
            }

            _isOpens[rid] = isOpen;
            _reference = _extension.Reference;

            // Drop down property context menu if there exist any.
            if (_propertyContextMenu != null)
            {
                var propertiesToUse = _contextMenuProperties.Where(x => _propertyContextMenuRect.Overlaps(x.Rect));
                int count = propertiesToUse.Count();
                bool hasMultiple = count > 1;
                foreach (var (rect, prop) in propertiesToUse)
                {
                    _propertyContextMenu.allowDuplicateNames = true;
                    OnPropertyContextMenu(_propertyContextMenu, prop, hasMultiple);

                    if (--count > 0)
                        _propertyContextMenu.AddSeparator("");
                }

                _propertyContextMenu?.DropDown(_propertyContextMenuRect);
            }

            _propertyContextMenu = null;
            _propertyContextMenuRect = default;
            _contextMenuProperties.Clear();

            void ShowHeaderContextMenu(Rect rect)
            {
                var menu = new GenericMenu();
                if (collection != null)
                {
                    menu.AddItem(new GUIContent("Reset"), false, () =>
                    {
                        var newExtension = (IExtension)Activator.CreateInstance(extensionType);
                        newExtension.Reference = _extension.Reference;
                        property.managedReferenceValue = newExtension;

                    });
                    if (_reference != null)
                    {
                        menu.AddItem(new GUIContent($"Modified Extension/Apply to Object '{_reference.name}'"), false, () =>
                        {

                        });
                    }
                    menu.AddItem(new GUIContent("Modified Extension/Revert"), false, () =>
                    {

                    });
                    menu.AddSeparator(null);
                    menu.AddItem(new GUIContent("Remove Extension"), false, () =>
                    {
                        Undo.RecordObject(_target, $"Removed Extension '{Extensions.Names[extensionType]}'");
                        collection.RemoveExtension(extensionType);
                        EditorUtility.SetDirty(_target);

                    });
                    menu.AddItem(new GUIContent("Move Up"), false, () =>
                    {
                        Undo.RecordObject(_target, $"Moved Up Extension '{Extensions.Names[extensionType]}'");
                        collection.MoveUpExtension(extensionType);
                        EditorUtility.SetDirty(_target);
                    });
                    menu.AddItem(new GUIContent("Move Down"), false, () =>
                    {
                        Undo.RecordObject(_target, $"Moved Down Extension '{Extensions.Names[extensionType]}'");
                        collection.MoveDownExtension(extensionType);
                        EditorUtility.SetDirty(_target);
                    });
                }
                menu.DropDown(rect);
            }
        }

        public void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
            => OnPropertyContextMenu(menu, property, false);
        public void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property, bool displayPropertyName)
        {
            string propertyDisplayName = displayPropertyName ? property.displayName + ": " : string.Empty;
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
                            FieldInfo info = _extension!.GetType()
                                .GetField(property.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            menu.AddItem(new GUIContent(propertyDisplayName + $"Apply to Reference '{_reference.name}'"), false, () =>
                            {
                                Undo.RecordObject(_reference, "Applied to Reference");
                                info.SetValue(otherExtension, info.GetValue(_extension));
                                EditorUtility.SetDirty(_reference);
                                serializedObject.Update();
                            });
                            menu.AddItem(new GUIContent(propertyDisplayName + "Revert"), false, () =>
                            {
                                Undo.RecordObject(_target, "Reverted Value");
                                info.SetValue(_extension, info.GetValue(otherExtension));
                                EditorUtility.SetDirty(_target);
                                serializedObject.Update();
                            });
                        }
                    }
                }
            }
        }

        internal void Internal_OnDrawBackground(TimelineClip clip, ClipBackgroundRegion region, IExtension extension)
        {
            OnDrawBackground(clip, region, extension);
        }
    }
}