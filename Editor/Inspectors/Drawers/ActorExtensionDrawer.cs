using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    [CustomPropertyDrawer(typeof(ActorExtension), true)]
    public class ActorExtensionDrawer : ExtensionDrawer
    {
        private string _runtimeActor;

        protected override void OnDrawProperties(Rect position, SerializedProperty property, GUIContent label, IExtension extension)
        {
            var serializedObject = property.serializedObject;
            var target = serializedObject.targetObject;
            var actorExtension = extension as ActorExtension;

            SerializedProperty textProperty = property.FindPropertyRelative("_editorText");

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Actor" : "Actor (Readonly)");
            EditorStyles.textField.wordWrap = true;

            if (EditorOrRuntime.IsEditor)
            {
                EditorGUI.BeginChangeCheck();
                string text = EditorGUILayout.TextArea(textProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    textProperty.stringValue = text;
                    serializedObject.ApplyModifiedProperties();
                    extension.SetModified(textProperty.name, true);
                    actorExtension.RefreshText();
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                if (!System.MemoryExtensions.Equals(_runtimeActor, actorExtension.RuntimeText, System.StringComparison.Ordinal))
                    _runtimeActor = actorExtension.RuntimeText.ToString();

                using (EditorGUIExtra.Disable.Scope())
                    EditorGUILayout.TextArea(_runtimeActor);
            }
            DrawModification(GUILayoutUtility.GetLastRect(), textProperty, extension);
        }
    }
}
