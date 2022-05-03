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
            Binder.OnEnterClip.Invoke(new DialogueSystemBinder.Callback
            {
                Index = index,
                Track = Track as DialogueTrack,
                Behaviour = behaviour,
                Binder = Binder,
            });
        }


        protected override void OnExitClip(Playable playable, DSPlayableBehaviour behaviour, FrameData info, object playerData)
        {
            int index = Binder.DialogueTracks.IndexOf(Track);
            Binder.OnExitClip.Invoke(new DialogueSystemBinder.Callback
            {
                Index = index,
                Track = Track as DialogueTrack,
                Behaviour = behaviour,
                Binder = Binder,
            });
        }
    }
}
