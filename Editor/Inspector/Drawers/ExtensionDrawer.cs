using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(Extension), true)]
    public class ExtensionDrawer : PropertyDrawer
    {
        private Dictionary<long, bool> _isOpens = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            long id = property.managedReferenceId;
            Type extensionType = property.managedReferenceValue.GetType();

            if (!_isOpens.ContainsKey(id))
                _isOpens.Add(id, false);

            EditorGUILayoutExtra.DrawUILine(color: new Color(0.102f, 0.102f, 0.102f), padding: -3);
            var color = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1.1f, 1.1f, 1.1f);
            var content = new GUIContent(Extensions.Names[extensionType]);
            Rect rect = GUILayoutUtility.GetRect(content, EditorStyles.foldoutHeader);
            rect.x += 14.0f;
            bool isOpen = EditorGUI.BeginFoldoutHeaderGroup(rect, _isOpens[id], content);
            EditorGUI.EndFoldoutHeaderGroup();
            EditorGUILayoutExtra.DrawUILine(
                color: isOpen ? new Color(0.19f, 0.19f, 0.19f) : new Color(0.102f, 0.102f, 0.102f), 
                padding: -4);
            GUI.backgroundColor = color;
            EditorGUI.indentLevel++;
            if (isOpen)
            {
                if (GetType() == typeof(ExtensionDrawer))             // If not inherited.
                {
                    IEnumerator enumerator = property.GetEnumerator();
                    int depth = property.depth;

                    while (enumerator.MoveNext())
                    {
                        property = enumerator.Current as SerializedProperty;

                        if (property == null || property.depth > depth + 1)
                            continue;

                        EditorGUILayout.PropertyField(property, true);
                    }
                }
            }
            EditorGUI.indentLevel--;
            _isOpens[id] = isOpen;
        }
    }
}