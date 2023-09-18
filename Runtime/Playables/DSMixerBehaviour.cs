using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DSMixerBehaviour : MixerBehaviourExtended
    {
        protected override void OnEnterClip(Playable playable, PlayableBehaviourExtended behaviour, FrameData info, object playerData)
        {
            var asset = (DSPlayableAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnEnter(playable, info, this, playerData);

            Binder.Internal_InvokeOnEnterDialogueClip(Track, behaviour);
        }

        protected override void OnProcessClip(Playable playable, PlayableBehaviourExtended behaviour, FrameData info, object playerData)
        {
            var asset = (DSPlayableAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnProcess(playable, info, this, playerData);

            Binder.Internal_InvokeOnProcessDialogueClip(Track, behaviour);
        }

        protected override void OnExitClip(Playable playable, PlayableBehaviourExtended behaviour, FrameData info, object playerData)
        {
            var asset = (DSPlayableAsset)behaviour.Asset;

            foreach (var extension in asset.Extensions)
                extension.OnExit(playable, info, this, playerData);

            Binder.Internal_InvokeOnExitDialogueClip(Track, behaviour);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Binder.Internal_InvokeOnDeleteTimeline();
        }
    }
}
