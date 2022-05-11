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
        public IReadOnlyList<IReadOnlyList<TimelineClip>> ActionClips => _actionGroupClips;
        public IReadOnlyList<int> ActionGroupCounts => _actionGroupCounts;

        private List<List<TimelineClip>> _actionGroupClips;
        private List<int> _actionGroupCounts;

        private int _currnetActionGroups;

        protected override void OnInterpret(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            IEnumerable<string> choiceTexts = ((JEnumerable<JToken>)currentNode.Values["_choices"]).Select(x => (string)((JProperty)x).Value);

            double start = default;
            double duration = default;

            DialogueInterpreter dialogueInterpreter = null;
            {
                if (previousNode.TryGetInterpreter(out var interpreter))    // Connected node.
                {
                    if (interpreter is DialogueInterpreter)
                    {
                        dialogueInterpreter = (DialogueInterpreter)interpreter;

                        // Extract time.
                        start = dialogueInterpreter.DialogueClip.start;
                        duration = dialogueInterpreter.DialogueClip.duration;
                    }
                }
            }

            if (dialogueInterpreter == null)
                return;

            string overrideSettingName = "default";
            if (!previousNode.Outputs.TryGetValue(0, out DSPort continueOutput)) // If continue port does not exist.
                overrideSettingName = "end";

            // 
            //  If previously connected.
            //
            if (_actionGroupClips != null && _actionGroupClips.Count > 0)
            {
                List<TimelineClip> newActions = new List<TimelineClip>();
                bool newGroup = false;
                foreach (var action in _actionGroupClips[_currnetActionGroups])
                {
                    // Extend clip if adjacent to the previous actions.
                    if (Math.Abs(action.end - dialogueInterpreter.DialogueClip.start) < 0.01) // Compare with tolerance
                    {
                        action.duration = dialogueInterpreter.DialogueClip.end - action.start;
                        ((ButtonAsset)action.asset).OverrideSettingName = overrideSettingName;
                        _actionGroupCounts[_currnetActionGroups]++;
                    }
                    else
                    {
                        newGroup = true;
                        // If separate, create new group;
                        var track = timeline.FindOrAllocateTrackSpace<ActionTrack>(start);
                        var clip = track.CreateClip<ButtonAsset>();
                        var asset = clip.asset as ButtonAsset;

                        clip.start = start;
                        clip.duration = duration;
                        asset.Text = ((ButtonAsset)action.asset).Text;              // Reuse the same text.
                        asset.System = system;
                        asset.Condition = ((ButtonAsset)action.asset).Condition;    // Reuse the same condition.
                        asset.OverrideSettingName = overrideSettingName;
                        asset.OnClick += ((ButtonAsset)action.asset).OnClick;

                        newActions.Add(clip);
                    }
                }

                if (newGroup)   // Add new group and increase current count.
                {
                    _currnetActionGroups++;
                    _actionGroupClips.Add(new List<TimelineClip>(newActions));  // Add all new actions.
                    _actionGroupCounts.Add(1);
                }
            }
            else
            {
                //
                // Create choice actions.
                //
                _actionGroupClips = new List<List<TimelineClip>>() { new List<TimelineClip>() };
                _actionGroupCounts = new List<int> { 1 };
                int index = 0; 
                foreach (string choiceText in choiceTexts)
                {
                    var track = timeline.FindOrAllocateTrackSpace<ActionTrack>(start);
                    var clip = track.CreateClip<ButtonAsset>();
                    var asset = clip.asset as ButtonAsset;

                    clip.start = start;
                    clip.duration = duration;
                    asset.Text = choiceText;
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

                    _actionGroupClips[_currnetActionGroups].Add(clip);

                    ++index;
                }
            }
        }

        protected override void OnNext(DSNode currentNode, DSNode previousNode, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            
        }
    }
}
