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
        public List<TimelineClip> ActionClips => _actionClips;

        private TimelineClip _dialogueClip;
        private List<TimelineClip> _actionClips;

        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            if (timeline.GetOutputTracks().OfType<DialogueTrack>().Count() <= 0)
                timeline.CreateTrack<DialogueTrack>();

            string text = (string)currentNode.Values["_text"];
            string actorID = (string)currentNode.Values["_actorID"];
            float speed = Convert.ToSingle(currentNode.Values["_speed"]);
            float endOffset = Convert.ToSingle(currentNode.Values["_endOffset"]);
            IEnumerable<string> choiceTexts = ((JEnumerable<JToken>)currentNode.Values["_choices"]).Select(x => (string)((JProperty)x).Value);

            IEnumerable<DialogueTrack> dialogueTracks = timeline.GetOutputTracks().OfType<DialogueTrack>();
            TrackAsset dialogueTrack = dialogueTracks.First();
            double maxEnd = dialogueTracks.Max(x => x.end);

            double duration = text.Length;
            double start = maxEnd; // Get max end before new clip.

            _dialogueClip = dialogueTrack.CreateClip<DialogueAsset>();

            {
                var asset = _dialogueClip.asset as DialogueAsset;

                asset.Text = text;
                asset.Actor = actorID;
                asset.Speed = speed;
                asset.EndOffset = endOffset;

                duration *= 1 / (speed * 15f);
                duration += endOffset;
            }

            _dialogueClip.start = start;
            _dialogueClip.duration = duration;


            //
            // Create choice actions.
            //
            _actionClips = new List<TimelineClip>();
            int index = 0;  // Input index.
            foreach (string choiceText in choiceTexts)
            {
                var track = timeline.FindOrAllocateTrackSpace<ActionTrack>(start);
                var clip = track.CreateClip<ButtonAsset>();
                var asset = clip.asset as ButtonAsset;

                clip.start = start;
                clip.duration = duration;

                asset.ButtonReference.exposedName = Guid.NewGuid().ToString();
                system.Director.SetReferenceValue(asset.ButtonReference.exposedName, system.Buttons[index]);
                asset.Text = choiceText;

                if (system.ActionOverrideSettings.Count >= index)
                    asset.Settings = system.ActionOverrideSettings[index];

                if (currentNode.Inputs.TryGetValue(index + 1, out DSPort conditionInputPort))   // Index 0 is "Connections".
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

        protected override void OnNext(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort outPort))
                nextNode = outPort.Connections.First().Input.Node;

            if (nextNode != null)
            {
                if (nextNode.TryGetInterpreter(out var interpreter))
                {
                    interpreter.OnInterpret(system);
                    interpreter.OnNext(system);
                }
            }
        }
    }
}
