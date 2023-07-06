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
            Dictionary<Guid, (string name, object value)> properties = new Dictionary<Guid, (string name, object value)>();
            Dictionary<Guid, DSNode> nodes = new Dictionary<Guid, DSNode>();
            Dictionary<string, List<DSNode>> propertyNodes = new Dictionary<string, List<DSNode>>();
            Dictionary<string, DSNode> inputNodes = new Dictionary<string, DSNode>();

            GraphSerialized graphData = DeserializeJSONContent(content);

            //
            // Properties
            //
            if (graphData.Properties != null)
            {
                for (int i = 0; i < graphData.Properties.Count; i++)
                {
                    JObject obj = graphData.Properties[i] as JObject;

                    Guid id = Guid.Empty;
                    {
                        if (obj.TryGetValue("ID", out JToken token))
                        {
                            string guidString = token.ToObject<string>();
                            if (!Guid.TryParseExact(guidString, "N", out id))
                                throw new DeserializeExpection("Unable to parse Guid: " + guidString);
                        }

                        if (id == Guid.Empty)
                            throw new DeserializeExpection("Unable to find blackboard property \"ID\"");
                    }

                    Type valueType = null;
                    {
                        if (obj.TryGetValue("Type", out JToken token))
                            valueType = Type.GetType(token.ToObject<string>());

                        if (valueType == null)
                            throw new DeserializeExpection("Unable to find blackboard property \"Type\"");
                    }

                    string name = null;
                    {
                        if (obj.TryGetValue("Name", out JToken token))
                            name = token.ToObject<string>();
                    }

                    object value = null;
                    {
                        if (obj.TryGetValue("Value", out JToken token))
                            value = token.ToObject(valueType);
                    }

                    properties.Add(id, (name, value));
                }
            }

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

                        if (assetType == typeof(ValueProcessor))
                        {
                            // if exposed property.
                            if (specialValues.TryGetValue("_propertyID", out object obj))
                            {
                                if (Guid.TryParseExact(obj as string, "N", out Guid propertyID))
                                {
                                    if (properties.TryGetValue(propertyID, out var property))
                                    {
                                        specialValues.Remove("_propertyID");
                                        specialValues.Add("_name", property.name);
                                        specialValues.Add("_value", property.value);

                                        if (!propertyNodes.ContainsKey(property.name))
                                            propertyNodes[property.name] = new List<DSNode>();

                                        propertyNodes[property.name].Add(node);
                                    }
                                }
                            }
                        }

                        if (assetType == typeof(InputInterpreter))                                      // Find all inputs.
                        {
                            if (specialData.TryGetValue("_id", out JToken token))
                                inputNodes.Add(token.ToObject<string>(), node);
                            else
                                throw new DeserializeExpection("\"_id\" not found.");
                        }

                        nodes.Add(id, node);
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

                if (!nodes.TryGetValue(outID, out DSNode outNode))
                    throw new Exception(outID + " was not found");

                if (!nodes.TryGetValue(inID, out DSNode inNode))
                    throw new Exception(inID + " was not found");

                DSPort outPort = outNode.InsertPort(outData.PortNumber, DSPort.Direction.Output);
                DSPort inPort = inNode.InsertPort(inData.PortNumber, DSPort.Direction.Input);

                outPort.ConnectTo(inPort);
            }

            Dictionary<string, object> simlipfiedProperties = new Dictionary<string, object>();
            foreach (var property in properties.Values)
                simlipfiedProperties.Add(property.name, property.value);

            DSGraph graph = new DSGraph()
            {
                _inputNodes = inputNodes,
                _properties = simlipfiedProperties,
                _propertyNodes = propertyNodes,
            };

            return graph;
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
            system.Director.time = 0;

            return timeline;
        }

        public class DeserializeExpection : Exception
        {
            public DeserializeExpection() { }
            public DeserializeExpection(string message) : base(message) { }
        }
    }
}
