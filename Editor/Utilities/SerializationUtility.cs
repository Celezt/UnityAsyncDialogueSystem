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
        public const string FILE_EXTENSION = ".dialoguegraph";

        public static ReadOnlySpan<char> Serialize(int version, GUID objectID, UQueryState<Node> nodes, UQueryState<Edge> edges)
        {
            List<NodeSerializeData> nodeSerializeData = new List<NodeSerializeData>();
            List<EdgeSerializeData> edgeSerializeData = new List<EdgeSerializeData>();
            List<SerializedVector2Int> positionData = new List<SerializedVector2Int>();
            
            nodes.ForEach(node =>
            {            
                if (node is DialogueGraphNode { } dgNode)
                {
                    positionData.Add(Vector2Int.RoundToInt(dgNode.GetPosition().position));
                    nodeSerializeData.Add(new NodeSerializeData
                    {
                        ID = dgNode.ID.ToString(),
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
                                Node = new NodeSerializeData{ ID = inNode.ID.ToString()},
                                PortNumber = inNode.inputContainer.IndexOf(edge.input)
                            },
                            OutputPort =
                            {
                                Node = new NodeSerializeData{ ID = outNode.ID.ToString()},
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
                SpecialProperties = new List<dynamic>(),
            };


            return JsonConvert.SerializeObject(graphSerializeData, Formatting.Indented);
        }

        public static void WriteToFile(ReadOnlySpan<char> serializedData)
        {

        }
    }
}
