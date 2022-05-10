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
        private DialogueInterpreter _interpreter;
        private List<SnappedTrack> _availableDialogueTracks;
        private List<SnappedTrack> _availableActionTracks;

        private struct SnappedTrack : IEquatable<SnappedTrack>
        {
            public TrackAsset Track;
            public double End;

            public bool Equals(SnappedTrack other) => other.Track == Track;

            public static implicit operator SnappedTrack(TrackAsset track) => new SnappedTrack { Track = track };
        }

        protected override void OnInterpret(DSNode currentNode, IReadOnlyList<DSNode> previousNodes, Dialogue dialogue, DialogueSystem system, TimelineAsset timeline)
        {
            DSNode nextNode = null;
            if (currentNode.Outputs.TryGetValue(0, out DSPort port))
            {
                nextNode = port.Connections.First().Input.Node;
            }

            if (nextNode == null)
                return;

            _availableDialogueTracks = new List<SnappedTrack>();
            _availableActionTracks = new List<SnappedTrack>();
            foreach (var track in timeline.GetOutputTracks())
            {
                // Preload before creating next clip.
                if (track is DialogueTrack)
                    _availableDialogueTracks.Add(new SnappedTrack { Track = track, End = track.end }); 
                else if (track is ActionTrack)
                    _availableActionTracks.Add(new SnappedTrack { Track = track, End = track.end });
            }

            if (nextNode.TryGetInterpreter(out var interpreter))
            {
                if (interpreter is DialogueInterpreter dialogueInterpreter)
                {
                    _interpreter = dialogueInterpreter;
                    _interpreter.OnInterpret(system);
                }
            }

            foreach (var track in timeline.GetOutputTracks())
            {
                // Add new created tracks.
                if (track is DialogueTrack)
                {
                    if (!_availableDialogueTracks.Contains(track))
                        _availableDialogueTracks.Add(track);
                }
                else if (track is ActionTrack)
                {
                    if (!_availableActionTracks.Contains(track))
                        _availableActionTracks.Add(track);
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
                //
                // Offset dialogue clip.
                //
                {
                    TrackAsset blendTrack = _availableDialogueTracks.FirstOrDefault(x => x.End <= _interpreter.DialogueClip.start - offsetBlend).Track;  // Find valid from before clip.
                    _availableDialogueTracks.Remove(blendTrack);   // Remove available if found.

                    if (blendTrack == null)
                        blendTrack = timeline.CreateTrack<DialogueTrack>();

                    if (_interpreter.DialogueClip.GetParentTrack() != blendTrack)
                        _interpreter.DialogueClip.MoveToTrack(blendTrack);

                    _interpreter.DialogueClip.start -= offsetBlend;  // Set blend offset.
                }

                //
                // Offset action clips.
                //
                {
                    foreach (var clip in _interpreter.ActionClips)
                    {
                        TrackAsset blendTrack = _availableActionTracks.FirstOrDefault(x => x.End <= clip.start - offsetBlend).Track;  // Find valid from before clip.
                        _availableActionTracks.Remove(blendTrack);   // Remove available if found.

                        if (blendTrack == null)
                            blendTrack = timeline.CreateTrack<ActionTrack>();

                        if (clip.GetParentTrack() != blendTrack)
                            clip.MoveToTrack(blendTrack);

                        clip.start -= offsetBlend;  // Set blend offset.
                    }
                }

                _interpreter.OnNext(system);
            }
        }
    }
}