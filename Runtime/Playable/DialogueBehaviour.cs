using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DialogueBehaviour : PlayableBehaviour
    {
        public double StartTime { get; internal set; }
        public double EndTime { get; internal set; }

        public virtual void ProcessMixerFrame(PlayableDirector playableDirector, Playable playable, FrameData info, object playerData) { }
        public virtual void PostMixerFrame(PlayableDirector playableDirector, Playable playable, FrameData info, object playerData) { }
    }
}
