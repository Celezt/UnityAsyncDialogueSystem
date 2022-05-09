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
        public bool IsProcessing => _dialogueTracks.Any(x => x.Mixer.IsProcessing);
        public IReadOnlyList<DialogueTrack> DialogueTracks => _dialogueTracks;
        public int TrackCount => _dialogueTracks.Count;

        public PlayableDirector Director
        {
            get => _director;
            internal set => _director = value;
        }

        public UnityEvent<Callback> OnEnterClip = new UnityEvent<Callback>();
        public UnityEvent<Callback> OnExitClip = new UnityEvent<Callback>();
        public UnityEvent<Callback> OnProcessClip = new UnityEvent<Callback>();

        private List<DialogueTrack> _dialogueTracks = new List<DialogueTrack>();
        private Dictionary<DSTrack, TrackProperties> _trackProperties = new Dictionary<DSTrack, TrackProperties>();

        private PlayableDirector _director;

        internal DialogueSystemBinder Add(DialogueTrack track)
        {
            if (_dialogueTracks.Contains(track))
                return this;

            _dialogueTracks.Add(track);

            _trackProperties[track] = new TrackProperties();

            return this;
        }

        internal bool Remove(DialogueTrack track)
        {
            _dialogueTracks.Remove(track);

            _trackProperties.Remove(track);

            return true;
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
            public DSPlayableBehaviour Behaviour { get; internal set; }
            public DialogueTrack Track { get; internal set; }
            public TimelineClip Clip => Behaviour.Clip;
            public PlayableDirector Director => Binder.Director;
            public Playable Playable { get; internal set; }
            public FrameData Info { get; internal set; }
        }
    }
}
