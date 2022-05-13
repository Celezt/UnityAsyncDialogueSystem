using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;
using System;

namespace Celezt.DialogueSystem
{
    [DisallowMultipleComponent]
    public class DialogueSystemBinder : MonoBehaviour
    {
        public PlayableDirector Director
        {
            get => _director;
            internal set => _director = value;
        }

        public UnityEvent<Callback> OnEnterDialogueClip = new UnityEvent<Callback>();
        public UnityEvent<Callback> OnExitDialogueClip = new UnityEvent<Callback>();
        public UnityEvent<Callback> OnProcessDialogueClip = new UnityEvent<Callback>();
        public UnityEvent OnDeleteTimeline = new UnityEvent();

        private Dictionary<DSTrack, TrackProperties> _trackProperties = new Dictionary<DSTrack, TrackProperties>();

        private PlayableDirector _director;

        internal DialogueSystemBinder Add(DialogueTrack track)
        {
            if (!_trackProperties.ContainsKey(track))
                _trackProperties[track] = new TrackProperties();

            return this;
        }

        internal bool Remove(DialogueTrack track)
        {
            return _trackProperties.Remove(track);
        }

        [Serializable]
        public class TrackProperties
        {
            public object UserData
            {
                get => _userData;
                set => _userData = value;
            }

            private object _userData;
        }

        public struct Callback
        {
            public int Index { get; internal set; }
            public double Time => Director.time;
            public double Start => Clip.start;
            public double End => Clip.end;
            public object UserData
            {
                get => Binder._trackProperties[Track].UserData;
                set => Binder._trackProperties[Track].UserData = value;
            }
            public DialogueSystemBinder Binder { get; internal set; }
            public DSPlayableAsset Asset { get; internal set; }
            public DSPlayableBehaviour Behaviour { get; internal set; }
            public DialogueTrack Track { get; internal set; }
            public TimelineClip Clip => Behaviour.Clip;
            public PlayableDirector Director => Binder.Director;
            public Playable Playable { get; internal set; }
            public FrameData Info { get; internal set; }
        }
    }
}
