using Celezt.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#nullable enable

namespace Celezt.DialogueSystem
{
    [Serializable]
    public abstract class Extension<T> : IExtension<T> where T : UnityEngine.Object, IExtensionCollection
    {
        private static readonly Dictionary<Type, string[]> _propertyNames = new();

        public T Asset => (_target as T)!;

        public IReadOnlyDictionary<string, bool> PropertiesModified
        {
            get
            {
                InitializePropertyModifiers();

                return _propertiesModified;
            }
        }

        UnityEngine.Object IExtension.Target
        {
            get => _target;
            set => _target = value;
        }

        UnityEngine.Object? IExtension.Reference
        {
            get => _reference;
            set
            {
                if (value != null && value is not IExtensionCollection)
                {
                    Debug.LogWarning($"Reference must be derived from {nameof(IExtensionCollection)}");
                    return;
                }

                var oldReference = _reference;
                _reference = value;

                Type type = GetType();

                if (oldReference != _reference)
                {
                    _extensionReference = null;

                    if (ExtensionReference != null)
                        UpdateProperties();

#if UNITY_EDITOR
                    UnityEditor.EditorUtility.SetDirty(_target);
#endif
                }
            }
        }

        public UnityEngine.Object? RootReference
        {
            get
            {
                Type type = GetType();

                UnityEngine.Object? GetReference(UnityEngine.Object? reference)
                {
                    if (ExtensionUtility.TryGetExtension(reference, type, out var extension))
                    {
                        if (extension.Reference != null)
                            return GetReference(extension.Reference);
                    }

                    return reference;
                }

                return GetReference(_reference);
            }
        }

        public IExtension? ExtensionReference
            => _extensionReference ??= ExtensionUtility.GetExtension(_reference, GetType());

        public IExtension? RootExtensionReference
           => ExtensionUtility.GetExtension(RootReference, GetType());

        public bool IsRoot => _reference == null;

        [SerializeField, HideInInspector]
        private UnityEngine.Object? _reference;

        [SerializeField, HideInInspector]
        private UnityEngine.Object _target = null!;

        [SerializeField, HideInInspector]
        private SerializableDictionary<string, bool> _propertiesModified = null!;

        private IExtension? _extensionReference;

        protected virtual void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip) { }
        protected virtual void OnEnter(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnProcess(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnExit(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnDestroy() { }

        public Extension()
        {
            Type type = GetType();
            if (!_propertyNames.ContainsKey(type))
                _propertyNames[type] = ReflectionUtility.GetSerializablePropertyNames(type).ToArray();
        }

        public void UpdateProperties()
        {
            IEnumerable<string> unmodifiedPropertyNames = PropertiesModified.Where(x => x.Value == false).Select(x => x.Key); // Get all unmodified properties.

            foreach (var currentExtension in this.GetDerivedExtensions())
            {
                IEnumerable<string> modifiedPropertyNames = unmodifiedPropertyNames.Where(x => currentExtension.GetModified(x));

                foreach (string propertyName in modifiedPropertyNames)   
                    currentExtension.CopyTo(this, propertyName);

                unmodifiedPropertyNames = unmodifiedPropertyNames.Except(modifiedPropertyNames);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_target);
#endif
        }

        public void SetModified(string propertyName, bool isModified)
        {
            InitializePropertyModifiers();

            if (_propertiesModified[propertyName] == isModified)
                return;

            _propertiesModified[propertyName] = isModified;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_target);
#endif
        }

        public bool GetModified(string propertyName)
            => PropertiesModified[propertyName];

        void IExtension.OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            UpdateProperties();
            OnCreate(graph, go, clip);
        }

        void IExtension.OnEnter(Playable playable , FrameData info, IPlayableBehaviour mixer, object playerData)
        {
            OnEnter(playable, info, (EMixerBehaviour)mixer, playerData);
        }

        void IExtension.OnProcess(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData)
        {
            OnProcess(playable,info, (EMixerBehaviour)mixer, playerData);
        }

        void IExtension.OnExit(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData)
        {
            OnExit(playable, info, (EMixerBehaviour)mixer, playerData);
        }

        void IExtension.OnDestroy()
        {
            OnDestroy();
        }

        private void InitializePropertyModifiers()
        {
            if (_propertiesModified == null)
            {
                _propertiesModified = new();
                Type type = GetType();
                foreach (var propertyName in _propertyNames[type])
                    _propertiesModified[propertyName] = false;
            }
        }
    }
}
