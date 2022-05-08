using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class InputInterpreter : AssetInterpreter
    {
        public override void OnImport(DSNode node, TimelineAsset timeline)
        {
            DSNode nextNode = node.Outputs[0].Connections.FirstOrDefault()?.Output.Node;

            if (nextNode == null)
                return;

            if (nextNode.TryGetInterpreter(out var interpreter))
            {
                interpreter.OnImport(node, timeline);
            }
            else if (nextNode.TryGetAllProcessors(out var processor))
            {

            }
        }
    }
}
