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
        /// <summary>
        /// Contains all the current directors that reference changed extension. When calling
        /// 'SetModifier', it rebuilds the graph after all changes have been done. The reason is to give the same
        /// effect as changing a serialized property on the clip itself, which rebuilds the graph on change.
        /// </summary>
        private static readonly HashSet<PlayableDirector> _toRebuild = new();
        /// <summary>
        /// Contains all serialized property names for each extension type that derives from <see cref="Extension{T}"/>.
        /// It is used to fill '_propertiesModified' with all properties that can be modified. Any property that is no longer
        /// in use will be removed by comparing to this dictionary if it has already been serialized.
        /// </summary>
        private static readonly Dictionary<Type, string[]> _propertyNames = new();

        public event Action<string> OnChangedCallback = delegate { };

        public T? Asset
        {
            get
            {
                T? asset = _target as T;

                if (asset == null)
                    return null;

                if (asset is EPlayableAsset { Clip: null }) // Return null if asset hasn't been initialised yet.
                    return null;

                return asset;
            }
        }

        public IReadOnlyDictionary<string, bool> PropertiesModified
        {
            get
            {
                InitializePropertyModifiers();
                return _propertiesModified;
            }
        }

        public UnityEngine.Object Target
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
                        ExtensionReference.OnChangedCallback -= Internal_OnChange;

                    _extensionReference = null;

                    if (ExtensionReference != null) // New reference extension.
                    {
                        ExtensionReference.OnChangedCallback += Internal_OnChange;
                        UpdateProperties();
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

        private bool _hasOnCreateFirstTime;

        protected virtual void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip) { }
        protected virtual void OnEnter(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnProcess(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnExit(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnChange(string propertyName) { }
        protected virtual void OnDestroy() { }

        public Extension()
        {
            Type type = GetType();
            if (!_propertyNames.ContainsKey(type))
                _propertyNames[type] = ReflectionUtility.GetSerializablePropertyNames(type).ToArray();
        }

        public void UpdateProperty(string propertyName)
        {
            if (GetModified(propertyName))  // Don't update if it property is modified.
                return;

            foreach (var currentExtension in this.GetDerivedExtensions())
            {
                if (currentExtension.IsRoot || currentExtension.GetModified(propertyName))
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
                IEnumerable<string> modifiedPropertyNames = unmodifiedPropertyNames.Where(x => currentExtension.IsRoot || currentExtension.GetModified(x));

                foreach (string propertyName in modifiedPropertyNames)   
                    currentExtension.CopyTo(this, propertyName);

                unmodifiedPropertyNames = unmodifiedPropertyNames.Except(modifiedPropertyNames);

                if (!unmodifiedPropertyNames.Any())
                    break;
            }
        }

        public void SetModified(bool isModified)
        {
            foreach (string property in PropertiesModified.Keys)
                Internal_SetModifier(property, isModified);

            foreach (PlayableDirector director in _toRebuild)
                director.RebuildGraph();

            _toRebuild.Clear();
        }
        public void SetModified(string propertyName, bool isModified)
        {
            Internal_SetModifier(propertyName, isModified);
            
            foreach (PlayableDirector director in _toRebuild)
                director.RebuildGraph();

            _toRebuild.Clear();
        }

        public bool GetModified(string propertyName)
            => PropertiesModified[propertyName];

        public void Awake()
        {
            if (ExtensionReference != null)
            {
                ExtensionReference.OnChangedCallback -= Internal_OnChange;
                ExtensionReference.OnChangedCallback += Internal_OnChange;
            }

            UpdateProperties();
        }

        void IExtension.OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            if (!_hasOnCreateFirstTime)
            {
                UpdateProperties();
                _hasOnCreateFirstTime = true;
            }

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
            {
#if UNITY_EDITOR
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

                    Awake();
                }
#endif
                return;
            }

            _propertiesModified = new();

            foreach (string propertyName in _propertyNames[GetType()])
                _propertiesModified[propertyName] = false;

            Awake();
        }

        internal void Internal_SetModifier(string propertyName, bool isModified)
        {
            InitializePropertyModifiers();

            OnChangedCallback(propertyName);

            if (_propertiesModified[propertyName] == isModified)
                return;

#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(_target, $"Change if property: '{propertyName}' is modified or not on: {_target}");
            UnityEditor.EditorUtility.SetDirty(_target);
#endif
            _propertiesModified[propertyName] = isModified;
        }

        internal void Internal_OnChange(string propertyName)
        {
            if (_target is EPlayableAsset asset)
                _toRebuild.Add(asset.Director);

            UpdateProperty(propertyName);
            OnChange(propertyName);
            OnChangedCallback(propertyName);
        }
    }
}
