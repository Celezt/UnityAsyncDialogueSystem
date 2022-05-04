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
        public SerializableDictionary<PlayableAsset, ActionBinder> ActionBinderDictionary => _actionBinderDictionary;

        public int TrackCount => _dialogueTracks.Count;

        public PlayableDirector Director
        {
            get => _director;
            internal set => _director = value;
        }
        
        public UnityEvent OnCreateTrackMixer;
        public UnityEvent<Callback> OnEnterClip;
        public UnityEvent<Callback> OnExitClip;
        public UnityEvent<Callback> OnProcessClip;

        [SerializeField, HideInInspector]
        private SerializableDictionary<PlayableAsset, ActionBinder> _actionBinderDictionary = new SerializableDictionary<PlayableAsset, ActionBinder>();

        private List<DialogueTrack> _dialogueTracks = new List<DialogueTrack>();
        private List<ActionTrack> _actionTracks = new List<ActionTrack>();
        private Dictionary<DSTrack, TrackProperties> _trackProperties = new Dictionary<DSTrack, TrackProperties>();

        private PlayableDirector _director;

        internal DialogueSystemBinder Add(DSTrack track)
        {
            switch (track)
            {
                case DialogueTrack:
                    {
                        if (_dialogueTracks.Contains(track))
                            return this;

                        _dialogueTracks.Add(track as DialogueTrack);
                        break;
                    }
                case ActionTrack:
                    {
                        if (_actionTracks.Contains(track))
                            return this;

                        _actionTracks.Add(track as ActionTrack);
                        break;
                    }
            }

            _trackProperties[track] = new TrackProperties();

            return this;
        }

        internal bool Remove(DSTrack track)
        {
            switch (track)
            {
                case DialogueTrack dialogueTrack:
                    {
                        _dialogueTracks.Remove(dialogueTrack);

                        break;
                    }
                case ActionTrack actionTrack:
                    {
                        _actionTracks.Remove(actionTrack);
                        break;
                    }
            }

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

        [Serializable]
        public struct ActionBinder
        {
            public UnityEvent OnEnter;
            public UnityEvent OnExit;
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
