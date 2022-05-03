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
        public IEnumerable<object> UserData => _userData;

        public int TrackCount => _dialogueTracks.Count;

        public PlayableDirector Director
        {
            get => _director;
            internal set => _director = value;
        }

        public UnityEvent OnCreateTrackMixer;
        public UnityEvent<Callback> OnEnterClip;
        public UnityEvent<Callback> OnExitClip;

        private List<DialogueTrack> _dialogueTracks = new List<DialogueTrack>();
        private List<object> _userData = new List<object>();

        private PlayableDirector _director;

        public object GetUserData(int index)
        {
            if (_userData.Count > index)
                return _userData[index];

            return null;
        }
        public void SetUserData(int index, object value)
        {
            if (_userData.Count > index)
                _userData[index] = value;
        }

        internal DialogueSystemBinder Add(DSTrack track)
        {
            if (track.GetType() == typeof(DialogueTrack))
            {
                if (_dialogueTracks.Contains(track))
                    return this;

                _dialogueTracks.Add(track as DialogueTrack);
                _userData.Add(null);
            }

            return this;
        }

        internal bool Remove(DSTrack track)
        {
            int index = _dialogueTracks.IndexOf(track);

            if (index != -1)
                return false;

            if (track is DialogueTrack)
                _dialogueTracks.RemoveAt(index);

            _userData.RemoveAt(index);

            return true;
        }

        public struct Callback
        {
            public int Index { get; internal set; }
            public object UserData
            {
                get => Binder.GetUserData(Index);
                set => Binder.SetUserData(Index, value);
            }
            public DSPlayableBehaviour Behaviour { get; internal set; }
            public DialogueTrack Track { get; internal set; }
            public DialogueSystemBinder Binder { get; internal set; }
        }
    }
}
