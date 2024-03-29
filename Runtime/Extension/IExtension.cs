using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#nullable enable

namespace Celezt.DialogueSystem
{
    public interface IExtension<T> : IExtension where T : UnityEngine.Object
    {
        public T? Asset { get; }
    }

    public interface IExtension
    {
        /// <summary>
        /// When a property has been changed. Returns property name.
        /// </summary>
        public event Action<string> OnChangedCallback;

        /// <summary>
        /// The <see cref="UnityEngine.Object"/> the extension is attached to.
        /// </summary>
        public UnityEngine.Object Target { get; set; }
        /// <summary>
        /// Parent <see cref="UnityEngine.Object"/>.
        /// </summary>
        public UnityEngine.Object? Reference { get; set; }
        /// <summary>
        /// Furthest <see cref="UnityEngine.Object"/> in the reference tree.
        /// </summary>
        public UnityEngine.Object? RootReference { get; }
        public IReadOnlyDictionary<string, bool> PropertiesModified { get; }

        public IExtension? ExtensionReference { get; }
        public IExtension? RootExtensionReference { get; }

        public bool IsRoot { get; }

        public void UpdateProperty(string propertyName);
        public void UpdateProperties();
        public void SetModified(bool isModified);
        public void SetModified(string propertyName, bool isModified);
        public bool GetModified(string propertyName);

        public void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip);
        public void OnEnter(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData);
        public void OnProcess(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData);
        public void OnExit(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData);
        public void OnDestroy();
        public void Awake();
    }
}
