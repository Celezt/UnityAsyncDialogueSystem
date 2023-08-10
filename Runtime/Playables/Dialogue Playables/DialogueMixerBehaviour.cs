using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using System.Threading.Tasks;
using System.Threading;

namespace Celezt.DialogueSystem
{
    public class DialogueMixerBehaviour : DSMixerBehaviour
    {
        private int _characterCount;
        private float _previousValue = float.MaxValue;

        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.VisibilityInterval * _characterCount;

            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);

            foreach (ITag tag in asset.TagSequence)
            {
                switch (tag)
                {
                    case TagSpan<DialogueAsset> tagRange:
                        break;
                    case TagSingle<DialogueAsset> tagMarker when
                    asset.StartOffset == 0 && tagMarker.Index == 0 && currentValue < 1 ||
                    asset.EndOffset == 0 && tagMarker.Index == _characterCount && currentValue > _characterCount - 1:
                        tagMarker.OnInvoke();
                        break;
                }
            }
        }

        protected override void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.VisibilityInterval * _characterCount;

            foreach (ITag tag in asset.TagSequence)
            {
                switch (tag)
                {
                    case TagSpan<DialogueAsset> tagRange:
                        break;
                    case TagSingle<DialogueAsset> tagMarker when IsPlayingForward ?
                    (tagMarker.Index == 0 && asset.StartOffset > 0 ?
                        _previousValue <= tagMarker.Index && currentValue > tagMarker.Index :
                        _previousValue < tagMarker.Index && currentValue >= tagMarker.Index) :
                    (tagMarker.Index == 0 && asset.StartOffset > 0 ?
                        _previousValue > tagMarker.Index && currentValue <= tagMarker.Index :
                        _previousValue >= tagMarker.Index && currentValue < tagMarker.Index):
                        tagMarker.OnInvoke();
                        break;
                }
            }

            _previousValue = currentValue;

            Binder.Internal_InvokeOnProcessDialogueClip(Track, behaviour);
        }

        protected override void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.VisibilityInterval * _characterCount;

            foreach (ITag tag in asset.TagSequence)
            {
                switch (tag)
                {
                    case TagSpan<DialogueAsset> tagRange:
                        break;
                    case TagSingle<DialogueAsset> tagMarker when 
                    asset.EndOffset == 0 && tagMarker.Index == _characterCount && currentValue > _characterCount - 1 ||
                    asset.StartOffset == 0 && tagMarker.Index == 0 && currentValue < 1:
                        tagMarker.OnInvoke();
                        break;
                }
            }

            Binder.Internal_InvokeOnExitDialogueClip(Track, behaviour);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Binder.Internal_InvokeOnDeleteTimeline();
        }
    }
}
