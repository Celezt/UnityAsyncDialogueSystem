using Celezt.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    [Serializable]
    public abstract class Extension<T> : IExtension<T> where T : UnityEngine.Object
    {
        public T Asset => _target as T;

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

        IReadOnlyDictionary<string, bool> IExtension.PropertiesModified => throw new NotImplementedException();

        T IExtension<T>.Asset => throw new NotImplementedException();

        [SerializeField, HideInInspector]
        private UnityEngine.Object _reference;

        [SerializeField, HideInInspector]
        private UnityEngine.Object _target;

        [SerializeField, HideInInspector]
        private SerializableDictionary<string, bool> _propertiesModified = new();

        protected virtual void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip) { }
        protected virtual void OnEnter(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnProcess(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }
        protected virtual void OnExit(Playable playable, FrameData info, EMixerBehaviour mixer, object playerData) { }

        void IExtension.OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
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
    }
}
