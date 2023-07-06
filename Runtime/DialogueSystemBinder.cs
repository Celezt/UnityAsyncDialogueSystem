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
            get
            {
                if (_director == null)
                    _director = GetComponent<PlayableDirector>();

                return _director;
            }
        }

        public event Action<Callback> OnEnterDialogueClipCallback = delegate { };
        public event Action<Callback> OnExitDialogueClipCallback = delegate { };
        public event Action<Callback> OnProcessDialogueClipCallback = delegate { };
        public event Action OnDeleteTimelineCallback = delegate { };

        [SerializeField] private UnityEvent<Callback> OnEnterDialogueClip = new UnityEvent<Callback>();
        [SerializeField] private UnityEvent<Callback> OnExitDialogueClip = new UnityEvent<Callback>();
        [SerializeField] private UnityEvent<Callback> OnProcessDialogueClip = new UnityEvent<Callback>();
        [SerializeField] private UnityEvent OnDeleteTimeline = new UnityEvent();

        private Dictionary<DSTrack, object> _trackProperties = new Dictionary<DSTrack, object>();

        private PlayableDirector _director;

        public readonly struct Callback
        {
            /// <summary>
            /// [0-1]. Unaffected by speed.
            /// </summary>
            public float PercentageUnscaled => 
                Mathf.Clamp01((float)((Time - Start) / (End - Start)));

            /// <summary>
            /// [0-1]
            /// </summary>
            public float Percentage =>
                Asset is ITime asset ? 
                asset.TimeSpeed.Evaluate(Mathf.Clamp01((float)((Time - Start + asset.StartOffset) / (End - asset.EndOffset - Start)))) : PercentageUnscaled;

            public double Time => Director.time;
            public double Start => Clip.start;
            public double End => Clip.end;
            public object UserData
            {
                get => Binder._trackProperties[Track];
                set => Binder._trackProperties[Track] = value; 
            }

            public readonly int Index;
            public readonly DialogueSystemBinder Binder;
            public readonly PlayableDirector Director;
            public readonly DSPlayableAsset Asset;
            public readonly DSPlayableBehaviour Behaviour;
            public readonly DSTrack Track;
            public readonly TimelineClip Clip;

            internal Callback(DialogueSystemBinder binder, DSTrack track, DSPlayableBehaviour behaviour)
            {
                Binder = binder;
                Director = binder.Director;
                Asset = behaviour.Asset;
                Track = track;
                Behaviour = behaviour;
                Clip = behaviour.Clip;

                TimelineAsset timeline = Director.playableAsset as TimelineAsset;
                Index = timeline.IndexOf(Track);
            }
        }

        internal void Internal_InvokeOnEnterDialogueClip(DSTrack track, DSPlayableBehaviour behaviour)
        {
            OnEnterDialogueClip.Invoke(new Callback(this, track, behaviour));
            OnEnterDialogueClipCallback(new Callback(this, track, behaviour));
        }

        internal void Internal_InvokeOnExitDialogueClip(DSTrack track, DSPlayableBehaviour behaviour)
        {
            OnExitDialogueClip.Invoke(new Callback(this, track, behaviour));
            OnExitDialogueClipCallback(new Callback(this, track, behaviour));
        }

        internal void Internal_InvokeOnProcessDialogueClip(DSTrack track, DSPlayableBehaviour behaviour)
        {
            OnProcessDialogueClip.Invoke(new Callback(this, track, behaviour));
            OnProcessDialogueClipCallback(new Callback(this, track, behaviour));
        }

        internal void Internal_InvokeOnDeleteTimeline()
        {
            OnDeleteTimeline.Invoke();
            OnDeleteTimelineCallback();
        }

        internal DialogueSystemBinder Internal_Add(DialogueTrack track)
        {
            if (!_trackProperties.ContainsKey(track))
                _trackProperties.Add(track, null);

            return this;
        }

        internal bool Internal_Remove(DialogueTrack track)
        {
            return _trackProperties.Remove(track);
        }

        private void Start()
        {
            _director = GetComponent<PlayableDirector>();
        }
    }
}
