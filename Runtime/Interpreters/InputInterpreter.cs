using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class InputInterpreter : AssetInterpreter
    {
        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            for (int i = 0; i < 2; i++)
                timeline.CreateTrack<DialogueTrack>();

            for (int i = 0; i < 6; i++)
                timeline.CreateTrack<ActionTrack>();

            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort outPort))
                nextNode = outPort.Connections.FirstOrDefault()?.Input.Node;

            if (nextNode == null)
                return;

            if (nextNode.TryGetInterpreter(out var interpreter))
            {
                interpreter.OnInterpret(system);
            }
        }
    }
}
