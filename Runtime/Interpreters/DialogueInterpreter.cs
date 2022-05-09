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
        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode previousNode = previousNodes.Last();

            List<DialogueTrack> dialogueTracks = new List<DialogueTrack>();
            List<ActionTrack> actionTracks = new List<ActionTrack>();

            foreach (var track in timeline.GetOutputTracks())
            {
                if (track is DialogueTrack)
                    dialogueTracks.Add((DialogueTrack)track);
                else if (track is ActionTrack)
                    actionTracks.Add((ActionTrack)track);
            }

            TimelineClip previousClip = dialogueTracks.First().GetClips().LastOrDefault();
            TimelineClip dialogueClip = dialogueTracks.First().CreateClip<DialogueAsset>();

            string text = (string)currentNode.Values["_text"];
            string actorID = (string)currentNode.Values["_actorID"];
            float speed = Convert.ToSingle(currentNode.Values["_speed"]);
            float endOffset = Convert.ToSingle(currentNode.Values["_endOffset"]);
            IEnumerable<string> choiceTexts = ((JEnumerable<JToken>)currentNode.Values["_choices"]).Select(x => (string)((JProperty)x).Value);

            double start = previousClip?.end ?? 0;
            double duration = text.Length;

            {
                var asset = dialogueClip.asset as DialogueAsset;

                asset.Text = text;
                asset.Actor = actorID;
                asset.Speed = speed;
                asset.EndOffset = endOffset;

                duration *= 1 / (speed * 15f);
                duration += endOffset;
            }

            dialogueClip.start = start;
            dialogueClip.duration = duration;

            int index = 0;
            foreach (string choiceText in choiceTexts)
            {
                var track = actionTracks[index];
                var buttonClip = track.CreateClip<ButtonAsset>();
                var asset = buttonClip.asset as ButtonAsset;

                buttonClip.start = start;
                buttonClip.duration = duration;

                asset.ButtonReference.exposedName = Guid.NewGuid().ToString();
                system.Director.SetReferenceValue(asset.ButtonReference.exposedName, system.Buttons[index]);
                asset.Text = choiceText;

                if (system.ActionOverrideSettings.Count >= index)
                    asset.Settings = system.ActionOverrideSettings[index];

                DSNode conditionNode = currentNode.Inputs[index + 1].Connections.FirstOrDefault()?.Output.Node;

                if (conditionNode.TryGetAllProcessors(out var processors))
                {
                    asset.Condition = processors.First();
                }

                ++index;
            }

            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort outPort))
                nextNode = outPort.Connections.FirstOrDefault()?.Input.Node;

            if (nextNode != null)
            {
                if (nextNode.TryGetInterpreter(out var interpreter))
                {
                    interpreter.OnInterpret(system);
                }
            }
        }
    }
}
