using Celezt.Timeline;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DSMixerBehaviour : MixerBehaviourExtended
    {
        private DialogueSystemBinder _binder;

        protected override void OnEnterClip(Playable playable, PlayableBehaviourExtended behaviour, FrameData info, object playerData)
        {
            _binder ??= (DialogueSystemBinder)playerData;
            var asset = (DSPlayableAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnEnter(playable, info, this, playerData);

            _binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);
        }

        protected override void OnProcessClip(Playable playable, PlayableBehaviourExtended behaviour, FrameData info, object playerData)
        {
            _binder ??= (DialogueSystemBinder)playerData;
            var asset = (DSPlayableAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnProcess(playable, info, this, playerData);

            _binder.Internal_InvokeOnProcessDialogueClip(Track, behaviour);
        }

        protected override void OnExitClip(Playable playable, PlayableBehaviourExtended behaviour, FrameData info, object playerData)
        {
            _binder ??= (DialogueSystemBinder)playerData;
            var asset = (DSPlayableAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnExit(playable, info, this, playerData);

            _binder.Internal_InvokeOnExitDialogueClip(Track, behaviour);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            _binder.Internal_InvokeOnDeleteTimeline();
        }
    }
}
