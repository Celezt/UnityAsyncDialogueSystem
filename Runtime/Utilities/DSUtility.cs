using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Celezt.DialogueSystem
{
    public static class DSUtility
    {
        /// <summary>
        /// Deserialize JSON content. Graph can be serialized.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static GraphSerialized DeserializeJSONContent(ReadOnlySpan<char> content)
        {
            return JsonConvert.DeserializeObject<GraphSerialized>(content.ToString());
        }

        /// <summary>
        /// Create dialogue system graph from JSON content for runtime use. Graph can NOT be serialized.
        /// </summary>
        /// <param name="content">JSON content.</param>
        /// <returns></returns>
        /// <exception cref="DeserializeExpection"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public static DSGraph CreateDSGraph(ReadOnlySpan<char> content)
        {
            Dictionary<Guid, DSNode> _assets = new Dictionary<Guid, DSNode>();
            Dictionary<string, DSNode> _inputs = new Dictionary<string, DSNode>();

            GraphSerialized graphData = DeserializeJSONContent(content);

            //
            // Nodes.
            //
            for (int i = 0; i < graphData.Nodes.Count; i++)
            {
                NodeSerialized nodeData = graphData.Nodes[i];
                JObject specialData = graphData.Specialization[i] as JObject;

                if (!Guid.TryParseExact(nodeData.ID, "N", out Guid id))
                    throw new DeserializeExpection(nodeData.ID + " is invalid GUID");

                Type assetType = Type.GetType(nodeData.Binder);
                if (assetType != null)
                {
                    if (typeof(IDSAsset).IsAssignableFrom(assetType))                                   // Is interpreter
                    {
                        Dictionary<string, object> specialValues = new Dictionary<string, object>();
                        foreach (JProperty property in specialData.Properties())                           // Get all special values.
                        {
                            if (property.Value is JValue)
                                specialValues.Add(property.Name, ((JValue)property.Value).Value);
                            else if (property.Value is JArray)
                                specialValues.Add(property.Name, ((JArray)property.Value).Values());
                            else
                                throw new DeserializeExpection(property.Value.Type + " is not supported");
                        }

                        DSNode node = new DSNode(assetType, specialValues);

                        if (assetType == typeof(InputInterpreter))                                      // Find all inputs.
                        {
                            if (specialData.TryGetValue("_id", out JToken token))
                                _inputs.Add(token.ToObject<string>(), node);
                            else
                                throw new DeserializeExpection("\"_id\" not found.");
                        }

                        _assets.Add(id, node);
                    }
                }
                else
                    throw new NullReferenceException("Asset type binder was not found");
            }

            //
            //  Edges and Ports.
            //
            for (int i = 0; i < graphData.Edges.Count; i++)
            {
                EdgeSerialized edgeData = graphData.Edges[i];
                PortSerialized outData = edgeData.OutputPort;
                PortSerialized inData = edgeData.InputPort;

                if (!Guid.TryParseExact(outData.NodeID, "N", out Guid outID))
                    throw new Exception(outData.NodeID + " is invalid GUID");

                if (!Guid.TryParseExact(inData.NodeID, "N", out Guid inID))
                    throw new Exception(inData.NodeID + " is invalid GUID");

                if (!_assets.TryGetValue(outID, out DSNode outNode))
                    throw new Exception(outID + " was not found");

                if (!_assets.TryGetValue(inID, out DSNode inNode))
                    throw new Exception(inID + " was not found");

                DSPort outPort = outNode.InsertPort(outData.PortNumber, DSPort.Direction.Output);
                DSPort inPort = inNode.InsertPort(inData.PortNumber, DSPort.Direction.Input);

                outPort.ConnectTo(inPort);
            }

            return new DSGraph(_inputs);
        }

        /// <summary>
        /// Create dialogue from input id.
        /// </summary>
        /// <param name="system">For dialogue system.</param>
        /// <param name="dialogue">Dialogue used.</param>
        /// <param name="inputID">Start position.</param>
        /// <returns>Created <see cref="TimelineAsset"/></returns>
        /// <exception cref="DeserializeExpection"></exception>
        public static TimelineAsset CreateDialogue(DialogueSystem system, Dialogue dialogue, string inputID)
        {
            if (!dialogue.Graph.InputNodes.TryGetValue(inputID, out DSNode inputNode))
                throw new DeserializeExpection($"Input ID: \"{inputID}\" does not exist in {dialogue.name}");

            if (inputNode.AssetType != typeof(InputInterpreter))
                throw new DeserializeExpection($"{inputNode.AssetType} is not {nameof(InputInterpreter)}");

            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = $"Dialogue - {dialogue.name}: {inputNode.Values["_id"]}";

            timeline.CreateTrack<DialogueTrack>();
            timeline.CreateTrack<DialogueTrack>();

            system.Director.playableAsset = timeline;
            system.Director.extrapolationMode = DirectorWrapMode.Hold;

            if (inputNode.TryGetInterpreter(out var interpreter))
            {
                interpreter.OnInterpret(system, null);
                interpreter.OnNext(system);
            }

            system.Director.RebuildGraph();

            return timeline;
        }

        /// <summary>
        /// Rebuild timeline with the next node being start position.
        /// </summary>
        /// <param name="system">For dialogue system.</param>
        /// <param name="nextNode">Start position.</param>
        /// <returns>Created <see cref="TimelineAsset"/></returns>
        public static TimelineAsset RebuildTimelineFromNode(this DSNode nextNode, DSNode previousNode, DialogueSystem system)
        {
            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            TimelineAsset oldTimeline = system.Director.playableAsset as TimelineAsset;
            timeline.name = oldTimeline.name;

            timeline.CreateTrack<DialogueTrack>();
            timeline.CreateTrack<DialogueTrack>();

            system.Director.playableAsset = timeline;
            system.Director.extrapolationMode = DirectorWrapMode.Hold;
            

            if (nextNode.TryGetInterpreter(out var interpreter))
            {
                interpreter.OnInterpret(system, previousNode);
                interpreter.OnNext(system);
            }

            system.Director.RebuildGraph();
            system.Director.Play();

            return timeline;
        }

#nullable enable
        /// <summary>
        /// Find track with available space.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// <param name="timeline">Find in timeline.</param>
        /// <param name="start">Minimum position.</param>
        /// <param name="reversed">If searching for the last track.</param>
        /// <returns>The available track. If not, return null.</returns>
        public static T? FindTrackSpace<T>(this TimelineAsset timeline, double start, bool reversed = false) where T : TrackAsset, new()
            => FindTrackSpace<T>(timeline.GetOutputTracks().OfType<T>(), start, reversed);

        /// <summary>
        /// Find track with available space.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// <param name="tracks">Find in tracks.</param>
        /// <param name="start">Minimum position.</param>
        /// <param name="reversed">If searching for the last track.</param>
        /// <returns>The available track. If not, return null.</returns>
        public static T? FindTrackSpace<T>(IEnumerable<T> tracks, double start, bool reversed = false) where T : TrackAsset, new()
        {
            return reversed ? tracks.LastOrDefault(x => x.end <= start) : tracks.FirstOrDefault(x => x.end <= start); // Get the first or last instance.
        }

        /// <summary>
        /// Find track with available space or allocate new track of that type.
        /// </summary>
        /// <typeparam name="T">Track type.</typeparam>
        /// <param name="timeline">Find in timeline.</param>
        /// <param name="start">Minimum position.</param>
        /// <param name="reversed">If searching for the last track.</param>
        /// <returns>The available track.</returns>
        public static T FindOrAllocateTrackSpace<T>(this TimelineAsset timeline, double start, bool reversed = false) where T : TrackAsset, new()
        {
            T? track = FindTrackSpace<T>(timeline, start, reversed);

            if (track == null)
                track = timeline.CreateTrack<T>();

            return track;
        }

        /// <summary>
        /// Find the index.
        /// </summary>
        /// <param name="timeline">Find in timeline.</param>
        /// <param name="track">Track to find the index for.</param>
        /// <returns>Index. -1 if not found.</returns>
        public static int IndexOf(this TimelineAsset timeline, TrackAsset track)
        {
            return timeline.GetOutputTracks().IndexOf(track);
        }

        public class DeserializeExpection : Exception
        {
            public DeserializeExpection() { }
            public DeserializeExpection(string message) : base(message) { }
        }
    }
}
