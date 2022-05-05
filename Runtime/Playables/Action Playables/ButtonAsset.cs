using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Celezt.DialogueSystem
{
    public class ButtonAsset : ActionAsset
    {
        protected override DSPlayableBehaviour CreateBehaviour(PlayableGraph graph, GameObject owner)
        {
            return new ButtonBehaviour();
        }
    }
}
