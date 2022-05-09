using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class DialogueBehaviour : DSPlayableBehaviour
    {
        public string Actor;
        [TextArea(10, int.MaxValue)]
        public string Text;

        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, TimelineClip clip)
        {
            if (!string.IsNullOrWhiteSpace(Text))
                clip.displayName = Text;
        }

    }
}
