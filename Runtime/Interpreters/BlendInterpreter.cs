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
        private List<DialogueTrack> _dialogueTracks = new List<DialogueTrack>();

        private AssetInterpreter _interpreter;
        private TimelineClip _previousClipFirst;

        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort port))
            {
                nextNode = port.Connections.First().Input.Node;
            }

            if (nextNode == null)
                return;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is DialogueTrack)
                    _dialogueTracks.Add((DialogueTrack)track);
            }

            _previousClipFirst = _dialogueTracks[0].GetClips().LastOrDefault();

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
                    TrackAsset blendTrack = null;
                    {
                        if (_previousClipFirst != null)  
                        {
                            if (_previousClipFirst.end <= dialogueInterpreter.DialogueClip.start - offsetBlend)  // If clip can fit with the offset.
                            {
                                blendTrack = _dialogueTracks[0];
                            }
                            else    // Use track 1 if nothing else works.
                            {
                                blendTrack = _dialogueTracks[1]; 
                            }
                        }
                        else
                        {
                            blendTrack = _dialogueTracks[0]; // Prioritize track 0 if empty.
                        }
                    }

                    if (blendTrack != null && dialogueInterpreter.DialogueClip.GetParentTrack() != blendTrack)
                        dialogueInterpreter.DialogueClip.MoveToTrack(blendTrack);

                    dialogueInterpreter.DialogueClip.start -= offsetBlend;
                }

                _interpreter.OnNext(system);
            }
        }
    }
}