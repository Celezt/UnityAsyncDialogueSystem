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
        private List<ITag> _tags;
        private int _characterCount;
        private float _previousCurrentValue = float.MaxValue;

        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.Interval * _characterCount;

            _tags = Tags.GetTags(asset.RawText, out _characterCount, asset);
            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);

            foreach (ITag tag in _tags)
            {
                switch (tag)
                {
                    case TagSpan<DialogueAsset> tagRange:
                        break;
                    case TagSingle<DialogueAsset> tagMarker when
                    asset.StartOffset == 0 && tagMarker.Index == 0 && currentValue < 1 ||
                    asset.EndOffset == 0 && tagMarker.Index == _characterCount && currentValue > _characterCount - 1:
                        tagMarker.Internal_OnInvoke();
                        break;
                }
            }
        }

        protected override void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.Interval * _characterCount;

            foreach (ITag tag in _tags)
            {
                switch (tag)
                {
                    case TagSpan<DialogueAsset> tagRange:
                        break;
                    case TagSingle<DialogueAsset> tagMarker when IsPlayingForward ?
                    (tagMarker.Index == 0 && asset.StartOffset > 0 ?
                        _previousCurrentValue <= tagMarker.Index && currentValue > tagMarker.Index :
                        _previousCurrentValue < tagMarker.Index && currentValue >= tagMarker.Index) :
                    (tagMarker.Index == 0 && asset.StartOffset > 0 ?
                        _previousCurrentValue > tagMarker.Index && currentValue <= tagMarker.Index :
                        _previousCurrentValue >= tagMarker.Index && currentValue < tagMarker.Index):
                        tagMarker.Internal_OnInvoke();
                        break;
                }
            }

            _previousCurrentValue = currentValue;

            Binder.Internal_InvokeOnProcessDialogueClip(Track, behaviour);
        }

        protected override void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.Interval * _characterCount;

            foreach (ITag tag in _tags)
            {
                switch (tag)
                {
                    case TagSpan<DialogueAsset> tagRange:
                        break;
                    case TagSingle<DialogueAsset> tagMarker when 
                    asset.EndOffset == 0 && tagMarker.Index == _characterCount && currentValue > _characterCount - 1 ||
                    asset.StartOffset == 0 && tagMarker.Index == 0 && currentValue < 1:
                        tagMarker.Internal_OnInvoke();
                        break;
                }
            }

            _tags = null;

            Binder.Internal_InvokeOnExitDialogueClip(Track, behaviour);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Binder.Internal_InvokeOnDeleteTimeline();
        }
    }
}
