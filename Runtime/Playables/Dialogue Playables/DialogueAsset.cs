using Celezt.Timeline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class DialogueAsset : DSPlayableAsset
    {
        protected override PlayableBehaviourExtended CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new DSPlayableBehaviour();
        }
    }
}
