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
        protected override void OnInterpret(DSNode node, DialogueSystem system, PlayableDirector director, TimelineAsset timeline)
        {
            DialogueTrack dialogueTrack = null;
            ActionTrack actionTrack = null;

            foreach (var track in timeline.GetOutputTracks().Reverse())
            {
                if (track is DialogueTrack)
                    dialogueTrack = (DialogueTrack)track;
                else if (track is ActionTrack)
                    actionTrack = (ActionTrack)track;
            }

            string text = (string)node.Values["_text"];
            string actorID = (string)node.Values["_actorID"];

            double start = director.time;
            double duration = text.Length * 0.2f;

            TimelineClip dialogueClip = dialogueTrack.CreateClip<DialogueAsset>();
            TimelineClip actionEventClip = actionTrack.CreateClip<ActionEventAsset>();

            dialogueClip.start = start;
            dialogueClip.duration = duration;

            actionEventClip.start = start;
            actionEventClip.duration = duration;

            {
                var asset = dialogueClip.asset as DialogueAsset;

                asset.Text = text;
                asset.Actor = actorID;
            }

            {
                var asset = actionEventClip.asset as ActionEventAsset;

                void OnContinue()
                {
                    DSNode nextNode = node.Outputs[0].Connections.FirstOrDefault()?.Input.Node; // Continue.

                    if (nextNode.TryGetInterpreter(out var interpreter))
                    {
                        interpreter.OnInterpret(system);
                    }

                    asset.OnExit.RemoveListener(OnContinue);
                }

                asset.OnExit.AddListener(OnContinue);

            }
        }
    }
}
