using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;
using System;
using Celezt.Timeline;

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

        private PlayableDirector _director;

        public readonly struct Callback
        {
            public readonly int TrackIndex;
            public readonly DialogueSystemBinder Binder;
            public readonly PlayableDirector Director;
            public readonly DSPlayableAsset Asset;
            public readonly ETrackAsset Track;

            internal Callback(DialogueSystemBinder binder, ETrackAsset track, EPlayableBehaviour behaviour)
            {
                Binder = binder;
                Director = binder.Director;
                Asset = (DSPlayableAsset)behaviour.Asset;
                Track = track;

                TimelineAsset timeline = Director.playableAsset as TimelineAsset;
                TrackIndex = timeline.IndexOf(Track);
            }
        }

        internal void Internal_InvokeOnEnterDialogueClip(ETrackAsset track, EPlayableBehaviour behaviour)
        {
            OnEnterDialogueClip.Invoke(new Callback(this, track, behaviour));
            OnEnterDialogueClipCallback(new Callback(this, track, behaviour));
        }

        internal void Internal_InvokeOnExitDialogueClip(ETrackAsset track, EPlayableBehaviour behaviour)
        {
            OnExitDialogueClip.Invoke(new Callback(this, track, behaviour));
            OnExitDialogueClipCallback(new Callback(this, track, behaviour));
        }

        internal void Internal_InvokeOnProcessDialogueClip(ETrackAsset track, EPlayableBehaviour behaviour)
        {
            OnProcessDialogueClip.Invoke(new Callback(this, track, behaviour));
            OnProcessDialogueClipCallback(new Callback(this, track, behaviour));
        }

        internal void Internal_InvokeOnDeleteTimeline()
        {
            OnDeleteTimeline.Invoke();
            OnDeleteTimelineCallback();
        }

        private void Start()
        {
            _director = GetComponent<PlayableDirector>();
        }
    }
}
