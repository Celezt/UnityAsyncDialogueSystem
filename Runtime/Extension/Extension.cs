using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public abstract class Extension<T> : IExtension<T> where T : UnityEngine.Object
    {
        public T Asset => (T)_target;

        public IReadOnlyDictionary<string, bool> PropertiesModified => _propertiesModified;

        UnityEngine.Object IExtension.Target
        {
            get => _target;
            set => _target = value;
        }

        UnityEngine.Object IExtension.Reference
        {
            get => _reference;
            set => _reference = value;
        }

        [SerializeField, HideInInspector]
        private UnityEngine.Object _reference;

        [SerializeField, HideInInspector]
        private UnityEngine.Object _target;

        [SerializeField, HideInInspector]
        private SerializableDictionary<string, bool> _propertiesModified = new();
    }
}
