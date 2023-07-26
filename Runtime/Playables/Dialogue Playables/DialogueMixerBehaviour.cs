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
        private int _textLength;

        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            _sequence = Tags.GetSequence(asset.RawText).ToList();
            _textLength = Tags.TextLengthWithoutTags(asset.Text);

            foreach (var tag in _sequence) 
                Debug.Log(tag);

            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);
        }

        protected override void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            int currentIndex = Mathf.CeilToInt(asset.Interval * _textLength);

            foreach (ITag tag in _sequence)
            {
                switch (tag)
                {
                    case Tag tagRange:
                        break;
                    case TagMarker tagMarker:
                        if ((IsPlayingForward && currentIndex >= tagMarker.Index) ||
                            (!IsPlayingForward && currentIndex <= tagMarker.Index))
                        {
                            Debug.Log(currentIndex);
                            //tagMarker.OnInvoke(currentIndex, asset);
                            //if (!_sequenceEnumerator.MoveNext())
                            //    _sequenceEnumerator = null;
                        }
                        break;
                }
            }

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
