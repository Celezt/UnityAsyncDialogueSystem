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
                if (node is DialogueGraphNode { } dgNode)
                {
                    positionData.Add(Vector2Int.RoundToInt(dgNode.GetPosition().position));
                    customSerializeData.Add(dgNode.GetCustomSaveData());
                    nodeSerializeData.Add(new NodeSerializeData
                    {
                        ID = dgNode.ID.ToString(),
                        Type = dgNode.GetType().FullName,
                    });
                }
            });

            edges.ForEach(edge =>
            {
                if (edge.input.node is DialogueGraphNode inNode)
                {
                    if (edge.output.node is DialogueGraphNode outNode)
                    {
                        edgeSerializeData.Add(new EdgeSerializeData
                        {
                            InputPort =
                            {
                                NodeID = inNode.ID.ToString(),
                                PortNumber = inNode.inputContainer.IndexOf(edge.input)
                            },
                            OutputPort =
                            {
                                NodeID = outNode.ID.ToString(),
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
    }
}
