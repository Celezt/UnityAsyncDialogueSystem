using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class BlendInterpreter : AssetInterpreter
    {
        private AssetInterpreter _interpreter;
        private IEnumerable<(double end, DialogueTrack track)> _availableTracks;

        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort port))
            {
                nextNode = port.Connections.First().Input.Node;
            }

            if (nextNode == null)
                return;

            _availableTracks = timeline.GetOutputTracks().OfType<DialogueTrack>().Select(x => (x.end, x));  // preload before creating next clip.

            if (nextNode.TryGetInterpreter(out _interpreter))
            {
                if (_interpreter.GetType() == typeof(DialogueInterpreter))
                {
                    _interpreter.OnInterpret(system);
                }
            }
        }

        protected override void OnNext(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode conditionNode = null;
            double offsetBlend = 0;
            if (currentNode.Inputs.TryGetValue(1, out DSPort valuePort))
            {
                DSPort conditionOutPort = valuePort.Connections.First().Output;
                conditionNode = conditionOutPort.Node;
                if (conditionNode.TryGetAllProcessors(out var processors))
                {
                    offsetBlend = Convert.ToDouble(processors.First().GetValue(conditionOutPort.Index));
                }
            }

            if (_interpreter != null)
            {
                if (_interpreter is DialogueInterpreter dialogueInterpreter)
                {
                    TrackAsset blendTrack = _availableTracks.FirstOrDefault(x => x.end <= dialogueInterpreter.DialogueClip.start - offsetBlend).track;  // Find valid from before clip.

                    if (blendTrack != null && dialogueInterpreter.DialogueClip.GetParentTrack() != blendTrack)
                        dialogueInterpreter.DialogueClip.MoveToTrack(blendTrack);

                    dialogueInterpreter.DialogueClip.start -= offsetBlend;
                }

                _interpreter.OnNext(system);
            }
        }
    }
}