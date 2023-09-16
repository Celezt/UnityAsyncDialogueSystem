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
        private float _previousValue = float.MaxValue;

        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnEnter(playable, info, this, playerData);

            //float currentValue = asset.VisibilityInterval * asset.Length;

            //foreach (ITag tag in asset.TagSequence)
            //{
            //    if (tag is ITagSpan tagSpan)
            //    {
            //        var range = tagSpan.Range;

            //        if (asset.StartOffset == 0 && range.start == 0 && currentValue < 1 ||
            //            asset.EndOffset == 0 && range.end == asset.Length && currentValue > asset.Length - 1)
            //            tagSpan.OnEnter();
            //    }
            //    else if (tag is ITagSingle tagSingle)
            //    {
            //        int index = tagSingle.Index;

            //        if (asset.StartOffset == 0 && index == 0 && currentValue < 1 ||
            //            asset.EndOffset == 0 && index == asset.Length && currentValue > asset.Length - 1)
            //            tagSingle.OnInvoke();
            //    }
            //}

            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);
        }

        protected override void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnProcess(playable, info, this, playerData);

            //float currentValue = asset.VisibilityInterval * asset.Length;

            //foreach (ITag tag in asset.TagSequence)
            //{
            //    if (tag is ITagSpan tagSpan)
            //    {
            //        var range = tagSpan.Range;

            //        if (IsPlayingForward ? 
            //            (range.start == 0 && asset.StartOffset > 0 ?
            //                _previousValue <= range.start && currentValue > range.start :
            //                _previousValue < range.start && currentValue >= range.start) :
            //            (range.end == 0 && asset.StartOffset > 0 ?
            //                _previousValue > range.end && currentValue <= range.end :
            //                _previousValue >= range.end && currentValue < range.end))
            //            tagSpan.OnEnter();
            //        else if (IsPlayingForward ?
            //            (range.end == 0 && asset.StartOffset > 0 ?
            //                _previousValue <= range.end && currentValue > range.end :
            //                _previousValue < range.end && currentValue >= range.end) :
            //            (range.start == 0 && asset.StartOffset > 0 ?
            //                _previousValue > range.start && currentValue <= range.start :
            //                _previousValue >= range.start && currentValue < range.start))
            //            tagSpan.OnExit();
            //        else if (currentValue > range.start && currentValue < range.end)
            //            tagSpan.OnProcess(Mathf.RoundToInt(currentValue));
            //    }
            //    else if (tag is ITagSingle tagSingle)
            //    {
            //        int index = tagSingle.Index;

            //        if (IsPlayingForward ?
            //        (index == 0 && asset.StartOffset > 0 ?
            //            _previousValue <= index && currentValue > index :
            //            _previousValue < index && currentValue >= index) :
            //        (index == 0 && asset.StartOffset > 0 ?
            //            _previousValue > index && currentValue <= index :
            //            _previousValue >= index && currentValue < index))
            //            tagSingle.OnInvoke();
            //    }
            //}

            //_previousValue = currentValue;

            Binder.Internal_InvokeOnProcessDialogueClip(Track, behaviour);
        }

        protected override void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            var asset = (DialogueAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnExit(playable, info, this, playerData);

            //float currentValue = asset.VisibilityInterval * asset.Length;

            //foreach (ITag tag in asset.TagSequence)
            //{
            //    if (tag is ITagSpan tagSpan)
            //    {
            //        var range = tagSpan.Range;

            //        if (asset.EndOffset == 0 && range.end == asset.Length && currentValue > asset.Length - 1 ||
            //            asset.StartOffset == 0 && range.start == 0 && currentValue < 1)
            //            tagSpan.OnExit();
            //    }
            //    else if (tag is ITagSingle tagSingle)
            //    {
            //        int index = tagSingle.Index;

            //        if (asset.EndOffset == 0 && index == asset.Length && currentValue > asset.Length - 1 ||
            //            asset.StartOffset == 0 && index == 0 && currentValue < 1)
            //            tagSingle.OnInvoke();
            //    }
            //}

            Binder.Internal_InvokeOnExitDialogueClip(Track, behaviour);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Binder.Internal_InvokeOnDeleteTimeline();
        }
    }
}
