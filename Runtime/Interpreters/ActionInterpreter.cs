using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class ActionInterpreter : AssetInterpreter
    {
        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            throw new System.NotImplementedException();
        }
    }
}
