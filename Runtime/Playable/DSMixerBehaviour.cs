using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DSMixerBehaviour : PlayableBehaviour
    {
        private List<BehaviourData> _oldBehaviours = new List<BehaviourData>();
        private List<BehaviourData> _currentBehaviours = new List<BehaviourData>();

        public DialogueSystemBinder Binder
        {
            get => _binder;
            internal set => _binder = value;
        }

        public DialogueTrack Track
        {
            get => _track;
            internal set
            {
                _track = value;
                _track.Mixer = this;
            }
        }

        public bool IsAvailable
        {
            get
            {
                return _currentBehaviours.Count == 0 || !_currentBehaviours.Any(x => x.Behaviour.ProcessState == DSPlayableBehaviour.ProcessStates.Processing);
            }
        }

        public List<DSPlayableBehaviour> CurrentBehaviours => _currentBehaviours.Select(x => x.Behaviour).ToList();

        private DialogueSystemBinder _binder;
        private DialogueTrack _track;

        private struct BehaviourData : IEquatable<BehaviourData>
        {
            public Playable Playable;
            public DSPlayableBehaviour Behaviour;

            public bool Equals(BehaviourData other) => Behaviour == other.Behaviour;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            ProcessClips(playable, info, playerData);
        }

        private void ProcessClips(Playable playable, FrameData info, object playerData)
        {
            _currentBehaviours.Clear();

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
                        _currentBehaviours.Add(new BehaviourData
                        {
                            Playable = currentPlayable,
                            Behaviour = input,
                        });
                    }
                }
            }

            for (int i = 0; i < _currentBehaviours.Count; i++)
                _oldBehaviours.Remove(_currentBehaviours[i]);// Remove until only clips no longer inside the scope is left.

            // Calls after exiting a clip.
            for (int i = 0; i < _oldBehaviours.Count; i++)
            {
                int index = _binder.Tracks.IndexOf(_track);
                _binder.OnExitClip.Invoke(new DialogueSystemBinder.Callback
                {
                    Index = index,
                    Track = _track,
                    Behaviour = _oldBehaviours[i].Behaviour,
                    Binder = _binder,
                });

                _oldBehaviours[i].Behaviour.ExitClip(_oldBehaviours[i].Playable, info, playerData as DialogueSystemBinder);
                _oldBehaviours[i].Behaviour.ProcessState = DSPlayableBehaviour.ProcessStates.None;
            }

            for (int i = 0; i < _currentBehaviours.Count; i++)
            {
                if (_currentBehaviours[i].Behaviour.ProcessState == DSPlayableBehaviour.ProcessStates.None)  // If not yet been processed.
                {
                    int index = _binder.Tracks.IndexOf(_track);
                    _binder.OnEnterClip.Invoke(new DialogueSystemBinder.Callback
                    {
                        Index = index,
                        Track = _track,
                        Behaviour = _currentBehaviours[i].Behaviour,
                        Binder = _binder,
                    });

                    _currentBehaviours[i].Behaviour.EnterClip(_currentBehaviours[i].Playable, info, playerData as DialogueSystemBinder);
                    _currentBehaviours[i].Behaviour.ProcessState = DSPlayableBehaviour.ProcessStates.Processing;
                }
            }

            _oldBehaviours = _currentBehaviours.ToList();
        }
    }
}
