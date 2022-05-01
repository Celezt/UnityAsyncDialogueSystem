using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;

namespace Celezt.DialogueSystem
{
    [DisallowMultipleComponent]
    public class DialogueSystemBinder : MonoBehaviour
    {
        public bool IsAvailable => _tracks.Any(x => x.Mixer.IsAvailable);
        public IReadOnlyList<DialogueTrack> Tracks => _tracks;

        public PlayableDirector Director
        {
            get => _director;
            internal set => _director = value;
        }

        public UnityEvent<Callback> OnEnterClip;
        public UnityEvent<Callback> OnExitClip;

        [System.NonSerialized]
        private List<DialogueTrack> _tracks = new List<DialogueTrack>();
        [System.NonSerialized]
        private List<object> _userData = new List<object>();

        private PlayableDirector _director;

        public object GetUserData(int index) => _userData[index];
        public void SetUserData(int index, object value) => _userData[index] = value;

        internal DialogueSystemBinder Add(DialogueTrack track)
        {
            if (_tracks.Contains(track))
                return this;

            _tracks.Add(track);
            _userData.Add(null);

            return this;
        }

        internal bool Remove(DialogueTrack track)
        {
            int index = _tracks.IndexOf(track);

            if (index != -1)
                return false;

            _tracks.RemoveAt(index);
            _userData.RemoveAt(index);

            return true;
        }

        public DialogueTrack this[int index] => Tracks[index];

        public struct Callback
        {
            public int Index { get; internal set; }
            public object UserData
            {
                get => Binder._userData[Index];
                set => Binder._userData[Index] = value;
            }
            public DSPlayableBehaviour Behaviour { get; internal set; }
            public DialogueTrack Track { get; internal set; }
            public DialogueSystemBinder Binder { get; internal set; }
        }
    }
}
