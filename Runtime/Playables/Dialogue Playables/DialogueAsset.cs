using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class DialogueAsset : DSPlayableAsset, ITime
    {
        public string Actor;
        [TextArea(10, int.MaxValue)]
        public string Text;
        [field: SerializeField]
        public AnimationCurve TimeSpeed { get; set; } = AnimationCurve.Linear(0, 0, 1, 1);
        [field: SerializeField, Min(0)]
        public float StartOffset { get; set; }
        [field: SerializeField, Min(0)]
        public float EndOffset { get; set; } = 1;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
