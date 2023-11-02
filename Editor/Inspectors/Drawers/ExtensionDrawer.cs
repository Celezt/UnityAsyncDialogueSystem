using Celezt.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#nullable enable

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(IExtension), true)]
    public class ExtensionDrawer : PropertyDrawer
    {
        private readonly static Dictionary<long, bool> _isOpens = new();

        private readonly List<(Rect Rect, SerializedProperty Property)> _contextMenuProperties = new();

        private UnityEngine.Object? _target;
        private IExtension? _extension;

        private GenericMenu? _propertyContextMenu;
        private Rect _propertyContextMenuRect;

        public ExtensionDrawer()
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        }

        ~ExtensionDrawer()
        {
            EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        }

        protected void DrawModification(Rect rect, SerializedProperty property, IExtension extension, bool clickable = true)
        {
            // If the current property is modified.
            if (extension.GetModified(property.name) && !extension.IsRoot)
            {
                rect.x = 0;
                rect.width = 20;

                if (clickable)
                    AddPropertyToContextMenu(rect, property);

                rect.width = 2;

                EditorGUI.DrawRect(rect, new Color(0.06f, 0.50f, 0.75f));
            }
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

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(currentProperty, true);
                if (EditorGUI.EndChangeCheck())
                    extension.SetModified(currentProperty.name, true);

                if (extension.Reference != null)
                    DrawModification(GUILayoutUtility.GetLastRect(), currentProperty, extension);
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
            GUIContent foldoutNameContent = new GUIContent(Extensions.Names[extensionType]);
            Rect foldoutRect = GUILayoutUtility.GetRect(foldoutNameContent, EditorStyles.foldoutHeader);
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
                    }
                }
            }

            bool isOpen = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, _isOpens[rid], foldoutNameContent, style, ShowHeaderContextMenu);
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

                // Skip if playable asset but hasn't yet been initialized.
                if (_target is not EPlayableAsset || ((EPlayableAsset)_target).Clip != null)
                    OnDrawProperties(position, property, label, _extension);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space(10);
            }

            // Drop down property context menu if there exist any.
            if (_propertyContextMenu != null)
            {
                _propertyContextMenu.allowDuplicateNames = true;
                var propertiesToUse = _contextMenuProperties.Where(x => _propertyContextMenuRect.Overlaps(x.Rect));
                int count = propertiesToUse.Count();
                bool hasMultiple = count > 1;
                foreach (var (rect, prop) in propertiesToUse)
                {
                    count--;

                    if (!OnPropertyContextMenu(_propertyContextMenu, prop, hasMultiple))
                        continue;

                    if (count > 0)
                        _propertyContextMenu.AddSeparator("");
                }

                _propertyContextMenu?.DropDown(_propertyContextMenuRect);
            }

            _propertyContextMenu = null;
            _propertyContextMenuRect = default;
            _contextMenuProperties.Clear();

            _isOpens[rid] = isOpen;

            void ShowHeaderContextMenu(Rect rect)
            {
                var menu = new GenericMenu();
                if (collection != null)
                {
                    // If any property is modified and has a reference.
                    if (!_extension.IsRoot && _extension.PropertiesModified.Any(x => x.Value == true))
                    {
                        menu.AddItem(new GUIContent($"Modified Extension/Apply to Reference '{_extension.Reference!.name}'"), false, () =>
                        {
                            _extension.CopyTo(_extension.ExtensionReference);
                            _extension.SetModified(false);
                            _extension.ExtensionReference!.SetModified(true);
                            serializedObject.Update();
                        });
                        if (!_extension.ExtensionReference!.IsRoot) // If reference is not the root.
                        {
                            menu.AddItem(new GUIContent($"Modified Extension/Apply to Root Reference '{_extension.RootReference!.name}'"), false, () =>
                            {
                                var rootExtensionReference = _extension.RootExtensionReference;
                                _extension.CopyTo(rootExtensionReference);
                                _extension.SetModified(false);
                                rootExtensionReference!.SetModified(true);
                                serializedObject.Update();
                            });
                        }

                        menu.AddItem(new GUIContent("Modified Extension/Revert"), false, () =>
                        {
                            _extension.ExtensionReference.CopyTo(_extension);
                            _extension.SetModified(false);
                            // To update possible visible clip changes. (Such as the clip name).
                            (_target as EPlayableAsset)?.Director.RebuildGraph();
                            serializedObject.Update();
                        });
                        menu.AddSeparator(null);
                    }
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

        public bool OnPropertyContextMenu(GenericMenu menu, SerializedProperty property, bool displayPropertyName)
        {
            var serializedObject = property.serializedObject;

            // If it is the current focused object.
            if (_target == null || _target != serializedObject.targetObject)
                return false;

            string? propertyDisplayName = displayPropertyName ? property.displayName + ": " : null;

            // Only add menu item if it is a property of the extension.
            string path = property.propertyPath;
            if (!path.StartsWith("_extensions.Array.data["))
                return false;

            if (path.AsSpan(path.IndexOf(']', 23) + 2).Any(x => x == '.'))  // Skip if it is a sub property.
                return false;

            IExtension? extensionReference = _extension?.ExtensionReference;
            if (extensionReference != null)
            {
                UnityEngine.Object reference = _extension!.Reference!;

                menu.AddItem(new GUIContent(propertyDisplayName + $"Apply to Reference '{reference.name}'"), false, () =>
                {
                    _extension.CopyTo(extensionReference, property.name);
                    _extension.SetModified(property.name, false);
                    extensionReference.SetModified(property.name, true);
                    serializedObject.Update();
                });
                if (!extensionReference.IsRoot) // If reference is not the root.
                {
                    menu.AddItem(new GUIContent(propertyDisplayName + $"Apply to Root Reference '{_extension.RootReference!.name}'"), false, () =>
                    {
                        var rootExtensionReference = _extension.RootExtensionReference;
                        _extension.CopyTo(rootExtensionReference, property.name);
                        _extension.SetModified(property.name, false);
                        rootExtensionReference!.SetModified(property.name, true);
                        serializedObject.Update();
                    });
                }
                menu.AddItem(new GUIContent(propertyDisplayName + "Revert"), false, () =>
                {
                    extensionReference.CopyTo(_extension, property.name);
                    _extension.SetModified(property.name, false);
                    // To update possible visible clip changes. (Such as the clip name).
                    (_target as EPlayableAsset)?.Director.RebuildGraph();
                    serializedObject.Update();
                });
            }

            return true;
        }
    }
}