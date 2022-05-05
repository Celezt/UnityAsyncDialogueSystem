using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ActionEventAsset : ActionAsset
    {
        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new ActionEventBehaviour();
        }

        private void OnDestroy()
        {
            if (BehaviourReference != null)
                (BehaviourReference as ActionEventBehaviour).Receiver?.ActionBinderDictionary.Remove(this);
        }
    }
}
