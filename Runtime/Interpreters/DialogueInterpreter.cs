using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Celezt.DialogueSystem
{
    public class DialogueInterpreter : AssetInterpreter
    {
        public TimelineClip DialogueClip => _dialogueClip;
        public IReadOnlyList<TimelineClip> ActionClips => _actionClips;

        private TimelineClip _dialogueClip;
        private List<TimelineClip> _actionClips;

        protected override void OnInterpret(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            if (timeline.GetOutputTracks().OfType<DialogueTrack>().Count() <= 0)
                timeline.CreateTrack<DialogueTrack>();

            string text = (string)currentNode.Values["_text"];
            string actorID = (string)currentNode.Values["_actorID"];
            float endOffset = Convert.ToSingle(currentNode.Values["_endOffset"]);
            AnimationCurve timeSpeed = (AnimationCurve)currentNode.Values["_timeSpeed"];
            float speed = Convert.ToSingle(currentNode.Values["_speed"]);
            int choiceCount = Convert.ToInt32(currentNode.Values["_choices"]);

            IEnumerable<DialogueTrack> dialogueTracks = timeline.GetOutputTracks().OfType<DialogueTrack>();
            TrackAsset dialogueTrack = dialogueTracks.First();
            double maxEnd = dialogueTracks.Max(x => x.end);

            double duration = text.Length;
            double start = maxEnd; // Get max end before new clip.

            _dialogueClip = dialogueTrack.CreateClip<DialogueAsset>();

            {
                var asset = _dialogueClip.asset as DialogueAsset;

                asset.RawText = text;
                asset.Actor = actorID;
                asset.TimeSpeedCurve = timeSpeed;
                asset.EndOffset = endOffset;

                duration *= 1 / (speed * 15f);
                duration += endOffset;
            }

            _dialogueClip.start = start;
            _dialogueClip.duration = duration;

            //
            //  Call vertical output nodes.
            //
            if (currentNode.Outputs.TryGetValue(-2, out DSPort verticalOutPort))
            {
                foreach (DSEdge edge in verticalOutPort.Connections)
                {
                    if (edge.Input.Node.TryGetInterpreter(out var interpreter))
                    {
                        interpreter.OnInterpret(system, this);
                        interpreter.OnNext(system);
                    }
                }
            }

            //
            // Create choice actions.
            //
            {
                string overrideSettingName = "default";
                if (!currentNode.Outputs.TryGetValue(0, out DSPort continueOutput)) // If continue port does not exist.
                {
                    overrideSettingName = "end";
                }

                _actionClips = new List<TimelineClip>();
                int index = 1;  // Index 0 is "Continue" and "Connections".
                foreach (DSPort port in currentNode.Outputs.Where(x => x.Key >= 0).Select(x => x.Value))
                {
                    // If connected node is not of type ChoiceInterpreter.
                    ChoiceInterpreter interpreter = null;
                    if (!port.Connections.FirstOrDefault()?.Input.Node.TryGetInterpreter(out interpreter) ?? true)
                        continue;

                    var track = timeline.FindOrAllocateTrackSpace<ActionTrack>(start);
                    var clip = track.CreateClip<ButtonAsset>();
                    var asset = clip.asset as ButtonAsset;

                    clip.start = start;
                    clip.duration = duration;
                    asset.Text = (string)(interpreter?.Node.Values["_text"] ?? index - 1);
                    asset.System = system;
                    asset.OverrideSettingName = overrideSettingName;

                    int snappedIndex = index;
                    asset.OnClick += () =>
                    {
                        if (currentNode.Outputs.TryGetValue(snappedIndex, out DSPort choiceOutputPort))
                        {
                            DSNode choiceNode = choiceOutputPort.Connections.First().Input.Node;

                            choiceNode.RebuildTimelineFromNode(this, system);
                        }
                    };

                    if (currentNode.Inputs.TryGetValue(index, out DSPort conditionInputPort))
                    {
                        DSNode conditionNode = conditionInputPort.Connections.First().Output.Node;

                        if (conditionNode.TryGetAllProcessors(out var processors))
                        {
                            asset.Condition = processors.First();
                        }
                    }

                    _actionClips.Add(clip);

                    ++index;
                }
            }            
        }

        protected override void OnNext(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort outPort))
                nextNode = outPort.Connections.First().Input.Node;

            if (nextNode != null)
            {
                if (nextNode.TryGetInterpreter(out var interpreter))
                {
                    interpreter.OnInterpret(system, this);
                    interpreter.OnNext(system);
                }
            }
        }
    }
}
