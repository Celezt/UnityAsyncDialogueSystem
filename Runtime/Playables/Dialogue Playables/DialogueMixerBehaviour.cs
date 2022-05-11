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
        protected override void OnEnterClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            int index = Binder.DialogueTracks.IndexOf(Track);
            Binder.OnEnterDialogueClip.Invoke(new DialogueSystemBinder.Callback
            {
                Index = index,
                Binder = Binder,
                Track = Track as DialogueTrack,
                Asset = behaviour.Asset,
                Behaviour = behaviour,
                Info = info,
                Playable = playable,
            });
        }

        protected override void OnProcessClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            int index = Binder.DialogueTracks.IndexOf(Track);
            Binder.OnProcessDialogueClip.Invoke(new DialogueSystemBinder.Callback
            {
                Index = index,
                Binder = Binder,
                Track = Track as DialogueTrack,
                Asset = behaviour.Asset,
                Behaviour = behaviour,
                Info = info,
                Playable = playable,
            });
        }


        protected override void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            int index = Binder.DialogueTracks.IndexOf(Track);
            Binder.OnExitDialogueClip.Invoke(new DialogueSystemBinder.Callback
            {
                Index = index,
                Binder = Binder,
                Track = Track as DialogueTrack,
                Asset = behaviour.Asset,
                Behaviour = behaviour,
                Info = info,
                Playable = playable,
            });
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Binder.OnDeleteTimeline.Invoke();
        }
    }
}
