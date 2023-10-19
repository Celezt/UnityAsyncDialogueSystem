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
    public abstract class Extension<T> : IExtension<T>, ISerializationCallbackReceiver where T : UnityEngine.Object, IExtensionCollection
    {
        private static readonly Dictionary<Type, string[]> _propertyNames = new();

        public event Action<string> OnChangedCallback = delegate { };

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
                    if (ExtensionReference != null) // Old reference extension.
                        ExtensionReference.OnChangedCallback -= OnChange;

                    _extensionReference = null;

                    if (ExtensionReference != null) // New reference extension.
                    {
                        ExtensionReference.OnChangedCallback += OnChange;
                        UpdateProperties();
                    }

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

#if UNITY_EDITOR
        private bool _isInitialized;
#endif

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

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            InitializePropertyModifiers();

            // If the extension already has been serialized to an asset but serialized properties has changed 
            if (!_isInitialized)
            {
                _isInitialized = true;
                string[] propertyNames = _propertyNames[GetType()];

                // Properties that does no longer exist.
                var toRemove = _propertiesModified.Keys.Except(propertyNames);

                foreach (string propertyName in toRemove)
                    _propertiesModified.Remove(propertyName);

                foreach (string propertyName in propertyNames)
                {
                    if (!_propertiesModified.ContainsKey(propertyName))
                        _propertiesModified[propertyName] = false;
                }
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (ExtensionReference != null)
            {
                ExtensionReference.OnChangedCallback -= OnChange;
                ExtensionReference.OnChangedCallback += OnChange;
            }
        }

        public void UpdateProperty(string propertyName)
        {
            if (GetModified(propertyName))  // Don't update if it property is modified.
                return;

            foreach (var currentExtension in this.GetDerivedExtensions())
            {
                if (currentExtension.GetModified(propertyName))
                {
                    currentExtension.CopyTo(this, propertyName);
                    break;
                }

            }
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

                if (!unmodifiedPropertyNames.Any())
                    break;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_target);
#endif
        }

        public void SetModified(string propertyName, bool isModified)
        {
            InitializePropertyModifiers();

            OnChangedCallback(propertyName);

            if (_propertiesModified[propertyName] == isModified)
                return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(_target, $"Changed if property: '{propertyName}' is modified or not on: {_target}");
#endif
            _propertiesModified[propertyName] = isModified;
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
            if (_propertiesModified is not null)
                return;

            _propertiesModified = new();

            foreach (string propertyName in _propertyNames[GetType()])
                _propertiesModified[propertyName] = false;
        }

        private void OnChange(string propertyName)
        {
            UpdateProperty(propertyName);
            OnChangedCallback(propertyName);
        }
    }
}
