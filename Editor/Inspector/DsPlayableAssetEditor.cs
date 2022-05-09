using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Celezt.DialogueSystem.Editor
{
    [CustomEditor(typeof(DSPlayableAsset), true), CanEditMultipleObjects]
    public class DSPlayableAssetEditor : UnityEditor.Editor
    {
        public virtual void BuildInspector() { }
 
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();

            BuildInspector();         

            serializedObject.ApplyModifiedProperties();
        }

        protected object GetValue(object instance, string name) => GetValue<object>(instance, name);
        protected T GetValue<T>(object instance, string name)
        {
            return (T)instance.GetType()
                .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .GetValue(instance);
        }
    }
}
