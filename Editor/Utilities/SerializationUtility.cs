using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class SerializationUtility
    {
        public static ReadOnlySpan<char> Serialize(int version, GUID objectID, UQueryState<Node> nodes, UQueryState<Edge> edges)
        {
            List<NodeSerializeData> nodeSerializeData = new List<NodeSerializeData>();
            List<EdgeSerializeData> edgeSerializeData = new List<EdgeSerializeData>();
            List<SerializedVector2Int> positionData = new List<SerializedVector2Int>();
            List<object> customSerializeData = new List<object>();
            
            nodes.ForEach(node =>
            {            
                if (node is CustomGraphNode { } dgNode)
                {
                    positionData.Add(Vector2Int.RoundToInt(dgNode.GetPosition().position));
                    customSerializeData.Add(dgNode.InternalGetSaveData());
                    nodeSerializeData.Add(new NodeSerializeData
                    {
                        ID = dgNode.Guid.ToString(),
                        Type = dgNode.GetType().FullName,
                    });
                }
            });

            edges.ForEach(edge =>
            {
                if (edge.input.node is CustomGraphNode inNode)
                {
                    if (edge.output.node is CustomGraphNode outNode)
                    {
                        edgeSerializeData.Add(new EdgeSerializeData
                        {
                            InputPort =
                            {
                                NodeID = inNode.Guid.ToString(),
                                PortNumber = inNode.inputContainer.IndexOf(edge.input)
                            },
                            OutputPort =
                            {
                                NodeID = outNode.Guid.ToString(),
                                PortNumber = outNode.outputContainer.IndexOf(edge.output)
                            }
                        });
                    }
                }
            });

            GraphSerializeData graphSerializeData = new GraphSerializeData
            {
                DGVersion = version,
                ObjectID = objectID.ToString(),
                Nodes = nodeSerializeData,
                Edges = edgeSerializeData,
                Positions = positionData,
                CustomSaveData = customSerializeData,
            };


            return JsonConvert.SerializeObject(graphSerializeData, Formatting.Indented);
        }

        public static ReadOnlySpan<char> Serialize(int version, GUID objectID)
        {
            GraphSerializeData graphSerializeData = new GraphSerializeData
            {
                DGVersion = version,
                ObjectID = objectID.ToString(),
                Nodes = new List<NodeSerializeData>(),
                Edges = new List<EdgeSerializeData>(),
                Positions = new List<SerializedVector2Int>(),
                CustomSaveData = new List<dynamic>(),
            };


            return JsonConvert.SerializeObject(graphSerializeData, Formatting.Indented);
        }

        public static GraphSerializeData Deserialize(ReadOnlySpan<char> content)
        {
            return JsonConvert.DeserializeObject<GraphSerializeData>(content.ToString());
        }

        internal static void Deserialize(this DialogueGraphView graphView, ReadOnlySpan<char> content)
        {
            GraphSerializeData deserializedData = Deserialize(content);

            // Load all nodes.
            int length = deserializedData.Nodes.Count;
            for (int i = 0; i < length; i++)
            {
                NodeSerializeData nodeData = deserializedData.Nodes[i];
                SerializedVector2Int positionData = deserializedData.Positions[i];
                object customData = deserializedData.CustomSaveData[i];

                if (!GUID.TryParse(nodeData.ID, out GUID guid))
                    throw new Exception(nodeData.ID + " is invalid GUID");

                CustomGraphNode graphNode = graphView.CreateNode(Type.GetType(nodeData.Type), positionData, guid);
                graphNode.InternalSetLoadData(customData);
                graphNode.InternalAfterLoad();
                graphView.AddElement(graphNode);
            }

            for (int i = 0; i < deserializedData.Edges.Count; i++)
            {
                EdgeSerializeData edgeData = deserializedData.Edges[i];
                PortSerializeData outputData = edgeData.OutputPort;
                PortSerializeData inputData = edgeData.InputPort;

                if (!GUID.TryParse(outputData.NodeID, out GUID outguid))
                    throw new Exception(outputData.NodeID + " is invalid GUID");

                if (!GUID.TryParse(inputData.NodeID, out GUID inguid))
                    throw new Exception(inputData.NodeID + " is invalid GUID");

                CustomGraphNode outNode = graphView.NodeDictionary[outguid];
                CustomGraphNode inNode = graphView.NodeDictionary[inguid];

                if (outNode.outputContainer.childCount < outputData.PortNumber)
                    throw new Exception("Trying to access output port that does not exist for " + outNode.GetType());

                if (inNode.inputContainer.childCount < inputData.PortNumber)
                    throw new Exception("Trying to access input port that does not exist for " + inNode.GetType());

                Edge edge = ((Port)outNode.outputContainer[outputData.PortNumber]).ConnectTo((Port)inNode.inputContainer[inputData.PortNumber]);

                graphView.AddElement(edge);
                outNode.RefreshPorts();
            }
        }
    }
}
