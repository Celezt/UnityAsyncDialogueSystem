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
    public class ActionInterpreter : AssetInterpreter
    {
        public List<TimelineClip> ActionClips => _actionClips;

        private List<TimelineClip> _actionClips;

        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            IEnumerable<string> choiceTexts = ((JEnumerable<JToken>)currentNode.Values["_choices"]).Select(x => (string)((JProperty)x).Value);

            double start = default;
            double duration = default;

            if (previousNodes.Last().TryGetInterpreter(out var interpreter))    // Connected node.
            {
                if (interpreter is DialogueInterpreter dialogueInterpreter)
                {      
                    // Extract time.
                    start = dialogueInterpreter.DialogueClip.start;
                    duration = dialogueInterpreter.DialogueClip.duration;
                }
                else
                {
                    return;
                }
            }

            //
            // Create choice actions.
            //
            _actionClips = new List<TimelineClip>();
            int index = 1;  // Input index. Index 0 is "Connections".
            foreach (string choiceText in choiceTexts)
            {
                var track = timeline.FindOrAllocateTrackSpace<ActionTrack>(start);
                var clip = track.CreateClip<ButtonAsset>();
                var asset = clip.asset as ButtonAsset;

                clip.start = start;
                clip.duration = duration;
                asset.Text = choiceText;
                asset.System = system;

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

        protected override void OnNext(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            
        }
    }
}
