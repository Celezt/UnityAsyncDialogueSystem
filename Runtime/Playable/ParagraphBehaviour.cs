using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ParagraphBehaviour : DSPlayableBehaviour
    {
        public string Actor;
        [TextArea]
        public string Text;

        public override void OnCreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount, TimelineClip clip)
        {
            if (!string.IsNullOrWhiteSpace(Text))
                clip.displayName = Text;
        }

    }
}
