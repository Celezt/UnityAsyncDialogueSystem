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
        private struct Choice
        {
            string Text;
        }

        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode previousNode = previousNodes.Last();

            DialogueTrack dialogueTrack = null;
            ActionTrack actionTrack = null;

            foreach (var track in timeline.GetOutputTracks())
            {
                if (dialogueTrack == null && track is DialogueTrack)
                    dialogueTrack = (DialogueTrack)track;
                else if (actionTrack == null && track is ActionTrack)
                    actionTrack = (ActionTrack)track;

                if (dialogueTrack != null && actionTrack != null)
                    break;
            }

            TimelineClip previousClip = dialogueTrack.GetClips().LastOrDefault();
            TimelineClip dialogueClip = dialogueTrack.CreateClip<DialogueAsset>();

            string text = (string)currentNode.Values["_text"];
            string actorID = (string)currentNode.Values["_actorID"];
            float speed = Convert.ToSingle(currentNode.Values["_speed"]);
            float endOffset = Convert.ToSingle(currentNode.Values["_endOffset"]);
            var choices = ((JEnumerable<JToken>)currentNode.Values["_choices"]).Select(x => x.Value<Choice>());

            double start = previousClip?.end ?? 0;
            double duration = text.Length;

            {
                var asset = dialogueClip.asset as DialogueAsset;

                asset.Text = text;
                asset.Actor = actorID;
                asset.Speed = speed;
                asset.EndOffset = endOffset;

                duration *= speed * 0.04f;
                duration += endOffset;
            }

            dialogueClip.start = start;
            dialogueClip.duration = duration;

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


            //{
            //    var asset = actionEventClip.asset as ActionEventAsset;

            //    void OnContinue()
            //    {
            //        DSNode nextNode = node.Outputs[0].Connections.FirstOrDefault()?.Input.Node; // Continue.

            //        if (nextNode.TryGetInterpreter(out var interpreter))
            //        {
            //            interpreter.OnInterpret(system);
            //        }

            //        asset.OnExit.RemoveListener(OnContinue);
            //    }

            //    asset.OnExit.AddListener(OnContinue);

            //}
        }
    }
}
