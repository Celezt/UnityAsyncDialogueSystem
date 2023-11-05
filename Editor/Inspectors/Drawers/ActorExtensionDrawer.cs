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

            SerializedProperty actorProperty = property.FindPropertyRelative("_editorActor");

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Actor" : "Actor (Readonly)");
            EditorStyles.textField.wordWrap = true;

            if (EditorOrRuntime.IsEditor)
            {
                EditorGUI.BeginChangeCheck();
                string text = EditorGUILayout.TextArea(actorProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    actorProperty.stringValue = text;
                    serializedObject.ApplyModifiedProperties();
                    extension.SetModified(actorProperty.name, true);
                    actorExtension.UpdateTags();
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
            DrawModification(GUILayoutUtility.GetLastRect(), actorProperty, extension);
        }
    }
}
