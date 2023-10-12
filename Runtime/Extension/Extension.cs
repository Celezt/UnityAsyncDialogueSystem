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
    public abstract class Extension<T> : ISerializationCallbackReceiver, IExtension<T> where T : UnityEngine.Object, IExtensionCollection
    {
        private static readonly Dictionary<Type, string[]> _propertyNames = new();

        public Box<int> SharedVersion => _sharedVersion;

        public int Version
        {
            get => _version;
            set => _version = value;
        }

        public int Linked
        {
            get => _linked;
            set
            {
                _linked = value;

#if UNITY_EDITOR
                UnityEditor.EditorUtility.IsDirty(_target);
#endif
            }
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
                var oldReference = _reference;
                _reference = value;

                Type type = GetType();

                if (oldReference != _reference)
                {
                    if (ExtensionUtility.TryGetExtensionFrom(oldReference, type, out var oldExtension))
                        oldExtension.Linked--;

                    if (_reference == null && _linked == 0)
                    {
                        _version = 0;
                        _sharedVersion = Box<int>.Empty;
                    }

                    if (ExtensionUtility.TryGetExtensionFrom(_reference, type, out var extension))
                    {
                        _sharedVersion = extension.SharedVersion;
                        _version = _sharedVersion.Value;
                        extension.Linked++;

                        ForceUpdateProperties();
                    }
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
                    if (ExtensionUtility.TryGetExtensionFrom(reference, type, out var extension))
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
            => ExtensionUtility.GetExtensionFrom(_reference, GetType());

        public IExtension? RootExtensionReference
           => ExtensionUtility.GetExtensionFrom(RootReference, GetType());

        public bool IsRoot => _reference == null;

        [SerializeField, HideInInspector]
        private UnityEngine.Object? _reference;

        [SerializeField, HideInInspector]
        private UnityEngine.Object _target = null!;

        [SerializeField, HideInInspector]
        private int _linked;

        [SerializeField, HideInInspector]
        private int _version;

        [SerializeField, HideInInspector]
        private Box<int> _sharedVersion;

        [SerializeField, HideInInspector]
        private SerializableDictionary<string, bool> _propertiesModified = null!;

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

        public void ForceUpdateProperties()
        {
            if (_reference is not IExtensionCollection referenceCollection)
                return;

            if (!referenceCollection.TryGetExtension(GetType(), out var referenceExtension))
                return;

            foreach (string propertyName in PropertiesModified.Where(x => !x.Value).Select(x => x.Key))   // Get all unmodified properties.
                referenceExtension.CopyTo(this, propertyName);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_target);
#endif
        }

        public void UpdateProperties()
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

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

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

        void IExtension.OnDestroy()
        {
            if (_reference != null)

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
