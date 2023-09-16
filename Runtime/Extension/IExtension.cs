using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public interface IExtension<T> : IExtension where T : UnityEngine.Object
    {
        public T Asset { get; }
    }

    public interface IExtension
    {
        public UnityEngine.Object Target { get; set; }
        public UnityEngine.Object Reference { get; set; }
        public IReadOnlyDictionary<string, bool> PropertiesModified { get; }

        public void OnCreate(PlayableGraph graph, GameObject go, TimelineClip clip);
        public void OnEnter(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData);
        public void OnProcess(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData);
        public void OnExit(Playable playable, FrameData info, IPlayableBehaviour mixer, object playerData);
    }
}
