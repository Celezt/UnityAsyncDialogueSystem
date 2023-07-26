using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DSMixerBehaviour : PlayableBehaviour
    {
        public DialogueSystemBinder Binder
        {
            get => _binder;
            internal set => _binder = value;
        }

        public DSTrack Track
        {
            get => _track;
            internal set
            {
                _track = value;
                _track.Mixer = this;
            }
        }

        public bool IsProcessing
        {
            get
            {
                return _currentClips.Any(x => x.Behaviour.ProcessState == DSPlayableBehaviour.ProcessStates.Processing);
            }
        }

        public bool IsPlayingForward => _isPLayingForward;

        public IEnumerable<DSPlayableBehaviour> CurrentBehaviours => _currentClips.Select(x => x.Behaviour);
        public IEnumerable<DSPlayableAsset> CurrentAssets => _currentClips.Select(x => x.Behaviour.Asset);

        private List<ClipData> _oldClips = new List<ClipData>();
        private List<ClipData> _currentClips = new List<ClipData>();

        private DialogueSystemBinder _binder;
        private DSTrack _track;

        private double _oldTime;
        private bool _isPLayingForward;

        protected struct ClipData : IEquatable<ClipData>
        {
            public Playable Playable;
            public DSPlayableBehaviour Behaviour;

            public bool Equals(ClipData other) => Behaviour == other.Behaviour;
        }

        protected virtual void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData) { }
        protected virtual void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData) { }
        protected virtual void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData) { }

        public sealed override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            ProcessClips(playable, info, playerData);
        }

        private void ProcessClips(Playable playable, FrameData info, object playerData)
        {
            _currentClips.Clear();

            double time = playable.GetTime();
            _isPLayingForward = time >= _oldTime;
            _oldTime = time;

            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                if (playable.GetInputWeight(i) > 0.0f)
                {
                    Playable currentPlayable = playable.GetInput(i);

                    if (!currentPlayable.GetPlayableType().IsSubclassOf(typeof(DSPlayableBehaviour)))
                        return;

                    DSPlayableBehaviour input = ((ScriptPlayable<DSPlayableBehaviour>)currentPlayable).GetBehaviour();

                    if (input != null)
                    {
                        _currentClips.Add(new ClipData
                        {
                            Playable = currentPlayable,
                            Behaviour = input,
                        });
                    }
                }
            }

            for (int i = 0; i < _currentClips.Count; i++)
                _oldClips.Remove(_currentClips[i]);// Remove until only clips no longer inside the scope is left.

            // Calls after exiting a clip.
            for (int i = 0; i < _oldClips.Count; i++)
            {
                OnExitClip(_oldClips[i].Playable, _oldClips[i].Behaviour, info, playerData);

                _oldClips[i].Behaviour.ExitClip(_oldClips[i].Playable, info, playerData as DialogueSystemBinder);
                _oldClips[i].Behaviour.ProcessState = DSPlayableBehaviour.ProcessStates.None;
            }

            for (int i = 0; i < _currentClips.Count; i++)
            {
                if (_currentClips[i].Behaviour.ProcessState == DSPlayableBehaviour.ProcessStates.None)  // If not yet been processed.
                {
                    OnEnterClip(_currentClips[i].Playable, _currentClips[i].Behaviour, info, playerData);

                    _currentClips[i].Behaviour.EnterClip(_currentClips[i].Playable, info, playerData as DialogueSystemBinder);
                    _currentClips[i].Behaviour.ProcessState = DSPlayableBehaviour.ProcessStates.Processing;
                }

                OnProcessClip(_currentClips[i].Playable, _currentClips[i].Behaviour, info, playerData);
            }

            if (_oldClips.Count <= _currentClips.Count) 
            {
                int index = 0;
                for (; index < _oldClips.Count; index++)
                    _oldClips[index] = _currentClips[index];

                for (; index < _currentClips.Count; index++)
                    _oldClips.Add(_currentClips[index]);
            }
            else
            {
                int index = 0;
                for (; index < _currentClips.Count; index++)
                    _oldClips[index] = _currentClips[index];

                _oldClips.RemoveRange(_currentClips.Count, _oldClips.Count - _currentClips.Count);
            }
        }
    }
}
