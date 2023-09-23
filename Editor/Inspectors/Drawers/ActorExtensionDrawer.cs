using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(ActorExtension), true)]
    public class ActorExtensionDrawer : ExtensionDrawer
    {
        private static readonly GUIContent _editorActorContent = new("Actor");
        private static readonly GUIContent _runtimeActorContent = new("Actor (Readonly)");

        private string _runtimeText;

        protected override void OnDrawProperties(Rect position, SerializedProperty property, GUIContent label, IExtension extension)
        {
            var serializedObject = property.serializedObject;
            var actorExtension = extension as ActorExtension;

            var actorProperty = property.FindPropertyRelative("_editorActor");

            if (EditorOrRuntime.IsEditor)
            {
                EditorGUILayout.LabelField(_editorActorContent);
                EditorStyles.textField.wordWrap = true;
                EditorGUI.BeginChangeCheck();
                var textProperty = property.FindPropertyRelative("_editorActor");
                textProperty.stringValue = EditorGUILayout.TextArea(textProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    actorExtension.RefreshText();
                }

                DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, actorProperty);
            }
            else
            {
                if (_runtimeText == null)
                    _runtimeText = actorExtension.RuntimeText.ToString();

                GUI.enabled = false;
                EditorGUILayout.LabelField(_runtimeActorContent);
                EditorStyles.textField.wordWrap = true;
                EditorGUILayout.TextArea(_runtimeText);
                GUI.enabled = true;

                DrawHasModification(GUILayoutUtility.GetLastRect(), extension.Reference, actorProperty);
            }
        }
    }
}
