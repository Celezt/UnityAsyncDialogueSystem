using Celezt.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

#nullable enable

namespace Celezt.DialogueSystem
{
    [Serializable]
    public abstract class Extension<T> : ISerializationCallbackReceiver, IExtension<T> where T : UnityEngine.Object, IExtensionCollection
    {
        private static readonly Dictionary<Type, string[]> _propertyNames = new();

        public int Version
        {
            get => _version;
            set => _version = value;
        }

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
                if (_reference != value)
                {
                    if (_reference is IExtensionCollection collection && collection.TryGetExtension(GetType(), out var extension)){
                        _version = extension.Version;
                        UpdateProperties(true);
                    }
                }

                _reference = value;

            }
        }

        public IExtension? ExtensionReference
        {
            get
            {
                if (_extensionReference == null)
                {
                    if (_reference is IExtensionCollection collection)
                        _extensionReference = collection.GetExtension(GetType());
                }

                return _extensionReference;
            }
        }

        [SerializeField, HideInInspector]
        private UnityEngine.Object? _reference;

        [SerializeField, HideInInspector]
        private UnityEngine.Object _target = null!;

        [SerializeField, HideInInspector]
        private int _version;

        [SerializeField, HideInInspector]
        private SerializableDictionary<string, bool> _propertiesModified = null!;

        private IExtension? _extensionReference;

        protected virtual void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip) { }
        protected virtual void OnEnter(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnProcess(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnExit(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }

        public Extension()
        {
            Type type = GetType();
            if (!_propertyNames.ContainsKey(type))
                _propertyNames[type] = ReflectionUtility.GetSerializablePropertyNames(type).ToArray();
        }

        public void UpdateProperties(bool forceUpdate = false)
        {
            if (_reference is not IExtensionCollection referenceCollection)
                return;

            if (!referenceCollection.TryGetExtension(GetType(), out var referenceExtension))
                return;

            int referenceVersion = referenceExtension.Version;

            if (referenceVersion == Version)
                return;

            if (referenceVersion < Version)
            {
                referenceExtension.Version = Version;
                referenceExtension.UpdateProperties();
            }
            else
                Version = referenceVersion;

            foreach (string propertyName in PropertiesModified.Where(x => x.Value == false).Select(x => x.Key))   // Get all unmodified properties.
                referenceExtension.CopyTo(this, propertyName);

#if UNITY_EDITOR
            EditorUtility.SetDirty(Asset);
#endif
        }

        public void SetModified(string propertyName, bool isModified)
        {
            InitializePropertyModifiers();
            if (PropertiesModified[propertyName] == isModified)
                return;

            _version++;

            _propertiesModified[propertyName] = isModified;

#if UNITY_EDITOR
            EditorUtility.SetDirty(Asset);
#endif
        }

        public bool GetModified(string propertyName)
            => PropertiesModified[propertyName];

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            Type type = GetType();
            foreach (var propertyName in _propertyNames[type])
            {
                if (!PropertiesModified.ContainsKey(propertyName))
                    _propertiesModified[propertyName] = false;
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }

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
