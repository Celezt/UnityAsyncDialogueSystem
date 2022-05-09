using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class DialogueAsset : DSPlayableAsset
    {
        public string Actor;
        [TextArea(10, int.MaxValue)]
        public string Text;
        public float Speed = 1.0f;
        public float EndOffset = 1.0f;

        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DialogueBehaviour();
        }
    }
}
