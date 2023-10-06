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
            var actorExtension = extension as ActorExtension;

            SerializedProperty actorProperty = property.FindPropertyRelative("_editorActor");

            EditorGUILayout.LabelField(EditorOrRuntime.IsEditor ? "Actor" : "Actor (Readonly)");
            EditorStyles.textField.wordWrap = true;

            if (EditorOrRuntime.IsEditor)
            {
                EditorGUI.BeginChangeCheck();
                actorProperty.stringValue = EditorGUILayout.TextArea(actorProperty.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    extension.SetModified(actorProperty.name, true);
                    actorExtension.RefreshText();
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
