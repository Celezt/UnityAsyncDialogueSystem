using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor.VersionControl;

namespace Celezt.DialogueSystem
{
    public class DialogueMixerBehaviour : DSMixerBehaviour
    {
        private List<ITag> _tags;
        private int _characterCount;
        private float _previousCurrentValue;

        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;
            float currentValue = asset.Interval * _characterCount;

            _tags = Tags.GetTags(asset.RawText, out _characterCount);
            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);

            foreach (ITag tag in _tags)
            {
                switch (tag)
                {
                    case TagElement tagRange:
                        break;
                    case TagMarker tagMarker when
                    asset.StartOffset == 0 && tagMarker.Index == 0 && currentValue < 1 ||
                    asset.EndOffset == 0 && tagMarker.Index == _characterCount && currentValue > _characterCount - 1:
                        tagMarker.OnInvoke(tagMarker.Index, asset);
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
                    case TagElement tagRange:
                        break;
                    case TagMarker tagMarker when IsPlayingForward ?
                    _previousCurrentValue < tagMarker.Index && currentValue >= tagMarker.Index:
                    _previousCurrentValue >= tagMarker.Index && currentValue < tagMarker.Index:
                        tagMarker.OnInvoke(tagMarker.Index, asset);
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
                    case TagElement tagRange:
                        break;
                    case TagMarker tagMarker when 
                    asset.EndOffset == 0 && tagMarker.Index == _characterCount && currentValue > _characterCount - 1 ||
                    asset.StartOffset == 0 && tagMarker.Index == 0 && currentValue < 1:
                        tagMarker.OnInvoke(tagMarker.Index, asset);
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
