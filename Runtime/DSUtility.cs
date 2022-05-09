using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using Newtonsoft.Json.Linq;

namespace Celezt.DialogueSystem
{
    public static class DSUtility
    {
        public static GraphSerialized DeserializeGraph(ReadOnlySpan<char> content)
        {
            return JsonConvert.DeserializeObject<GraphSerialized>(content.ToString());
        }

        public static DSGraph CreateRuntimeGraph(ReadOnlySpan<char> content)
        {
            Dictionary<Guid, DSNode> _assets = new Dictionary<Guid, DSNode>();
            Dictionary<string, DSNode> _inputs = new Dictionary<string, DSNode>();

            GraphSerialized graphData = DeserializeGraph(content);

            //
            // Nodes.
            //
            for (int i = 0; i < graphData.Nodes.Count; i++)
            {
                NodeSerialized nodeData = graphData.Nodes[i];
                JObject specialData = graphData.Specialization[i] as JObject;

                if (!Guid.TryParseExact(nodeData.ID, "N", out Guid id))
                    throw new Exception(nodeData.ID + " is invalid GUID");

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
                                throw new Exception(property.Value.Type + " is not supported");
                        }

                        DSNode node = new DSNode(assetType, specialValues);

                        if (assetType == typeof(InputInterpreter))                                      // Find all inputs.
                        {
                            if (specialData.TryGetValue("_id", out JToken token))
                                _inputs.Add(token.ToObject<string>(), node);
                            else
                                throw new Exception("\"_id\" not found.");
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

        public static TimelineAsset CreateTimeline(DialogueSystem system, DSNode inputNode)
        {
            if (inputNode.AssetType != typeof(InputInterpreter))
                throw new Exception($"{inputNode.AssetType} is not {nameof(InputInterpreter)}");

            TimelineAsset timeline = ScriptableObject.CreateInstance<TimelineAsset>();
            timeline.name = $"Dialogue - {inputNode.Values["_id"]}";

            system.Director.playableAsset = timeline;

            if (inputNode.TryGetInterpreter(out var interpreter))
            {
                interpreter.OnInterpret(system);
            }

            system.Director.Evaluate();

            return timeline;
        }
    }
}
