using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DialogueMixerBehaviour : DSMixerBehaviour
    {
        private List<ITag> _sequence;
        private int _characterCount;
        private float _previousCurrentValue;

        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            _sequence = Tags.GetSequence(asset.RawText, out _characterCount).ToList();
            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);
        }

        protected override void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            float currentValue = asset.Interval * (_characterCount + 1);
            int currentIndex = Mathf.CeilToInt(currentValue);

            foreach (ITag tag in _sequence)
            {
                switch (tag)
                {
                    case Tag tagRange:
                        break;
                    case TagMarker tagMarker:
                        if ((IsPlayingForward && _previousCurrentValue < tagMarker.Index && currentValue >= tagMarker.Index) ||
                            (!IsPlayingForward && _previousCurrentValue >= tagMarker.Index && currentValue < tagMarker.Index))
                        {
                            Debug.Log(currentValue + " " + tagMarker.Index);
                            //tagMarker.OnInvoke(currentIndex, asset);
                            //if (!_sequenceEnumerator.MoveNext())
                            //    _sequenceEnumerator = null;
                        }
                        break;
                }
            }

            _previousCurrentValue = currentValue;

            Binder.Internal_InvokeOnProcessDialogueClip(Track, behaviour);
        }

        protected override void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            _sequence = null;

            Binder.Internal_InvokeOnExitDialogueClip(Track, behaviour);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Binder.Internal_InvokeOnDeleteTimeline();
        }
    }
}
