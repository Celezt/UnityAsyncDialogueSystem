using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor.Utilities
{
    public static class JsonUtility
    {
        public static JObject ToJObject(object convert)
        {
            return JObject.FromObject(convert);
        }

        public static ReadOnlySpan<char> Serialize(object serialize)
        {
            return JsonConvert.SerializeObject(serialize);
        }

        public static object Deserialize(ReadOnlySpan<char> deserialize, Type type)
        {
            return JsonConvert.DeserializeObject(deserialize.ToString(), type);
        }

        public static ReadOnlySpan<char> SerializeGraph(int version, GUID objectID, DGView graphView)
        {
            List<NodeSerializeData> nodeData = new List<NodeSerializeData>();
            List<EdgeSerializeData> edgeData = new List<EdgeSerializeData>();
            List<SerializedVector2Int> positionData = new List<SerializedVector2Int>();
            List<object> specialData = new List<object>();
            List<object> propertyData = new List<object>();

            foreach (var property in graphView.Blackboard.Properties)
            {
                JObject obj = new JObject();
                obj.Add("Type", property.GetType().FullName); 
                obj.Add("Name", property.Name);
                obj.Add("Value", JToken.FromObject(property.Value));
                propertyData.Add(obj);
            }

            graphView.nodes.ForEach(node =>
            {            
                if (node is DGNode { } dgNode)
                {
                    positionData.Add(Vector2Int.RoundToInt(dgNode.GetPosition().position));
                    specialData.Add(GetFields(dgNode));
                    nodeData.Add(new NodeSerializeData
                    {
                        ID = dgNode.GUID.ToString(),
                        Type = dgNode.GetType().FullName,
                    });
                }
            });

            graphView.edges.ForEach(edge =>
            {
                if (edge.input.node is DGNode inNode)
                {
                    if (edge.output.node is DGNode outNode)
                    {
                        edgeData.Add(new EdgeSerializeData
                        {
                            InputPort =
                            {
                                NodeID = inNode.GUID.ToString(),
                                PortNumber = new Func<int>(() => {
                                    int inputNumber = inNode.inputContainer.IndexOf(edge.input);
                                    int verticalInputNumber = inNode.inputVerticalContainer.IndexOf(edge.input);
                                    if (inputNumber == -1)
                                    {
                                        if (verticalInputNumber == -1)
                                            throw new IndexOutOfRangeException("Tried to access non-existing input port");

                                        return (verticalInputNumber + 1) * -1; // Invert the value.
                                    }
                                    return inputNumber;
                                })()
                            },
                            OutputPort =
                            {
                                NodeID = outNode.GUID.ToString(),
                                PortNumber = new Func<int>(() => {
                                    int outputNumber = outNode.outputContainer.IndexOf(edge.output);
                                    int verticalOutputNumber = outNode.outputVerticalContainer.IndexOf(edge.output);
                                    if (outputNumber == -1)
                                    {
                                        if (verticalOutputNumber == -1)
                                            throw new IndexOutOfRangeException("Tried to access non-existing output port");

                                        return (verticalOutputNumber + 1) * -1; // Invert the value.
                                    }
                                    return outputNumber;
                                })()
                            }
                        });
                    }
                }
            });

            GraphSerializeData graphSerializeData = new GraphSerializeData
            {
                DGVersion = version,
                ObjectID = objectID.ToString(),
                Properties = propertyData,
                Nodes = nodeData,
                Edges = edgeData,
                Positions = positionData,
                CustomSaveData = specialData,
            };


            return JsonConvert.SerializeObject(graphSerializeData, Formatting.Indented);
        }

        public static ReadOnlySpan<char> SerializeGraph(int version, GUID objectID)
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

        public static GraphSerializeData DeserializeGraph(ReadOnlySpan<char> content)
        {
            return JsonConvert.DeserializeObject<GraphSerializeData>(content.ToString());
        }

        internal static void DeserializeGraph(this DGView graphView, ReadOnlySpan<char> content)
        {
            GraphSerializeData deserializedData = DeserializeGraph(content);

            foreach (object obj in deserializedData.Properties)
            {
                JObject jObj = (JObject)obj;
                Type type = null;
                {
                    if (jObj.TryGetValue("Type", out JToken token))
                    {
                        type = Type.GetType(token.ToObject<string>());
                        jObj.Remove("Type");
                    }
                }

                if (type == null)
                {
                    Debug.LogWarning("Unable to find blackboard property type");
                    continue;
                }

                IBlackboardProperty property = (IBlackboardProperty)Activator.CreateInstance(type);

                {
                    if (jObj.TryGetValue("Name", out JToken token))
                    {
                        property.Name = token.ToObject<string>();
                    }
                }
                {
                    if (jObj.TryGetValue("Value", out JToken token))
                    {
                        property.Value = token.ToObject(property.PropertyType);
                    }
                }

                graphView.Blackboard.AddProperty(property);
            }

            // Load all nodes.
            int length = deserializedData.Nodes.Count;
            for (int i = 0; i < length; i++)
            {
                NodeSerializeData nodeData = deserializedData.Nodes[i];
                SerializedVector2Int positionData = deserializedData.Positions[i];
                object customData = deserializedData.CustomSaveData[i];

                if (!GUID.TryParse(nodeData.ID, out GUID guid))
                    throw new Exception(nodeData.ID + " is invalid GUID");

                DGNode graphNode = graphView.CreateNode(Type.GetType(nodeData.Type), positionData, guid, (JObject)customData);
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

                DGNode outNode = graphView.NodeDictionary[outguid];
                DGNode inNode = graphView.NodeDictionary[inguid];

                if (outNode.outputContainer.childCount < outputData.PortNumber)
                    throw new Exception("Trying to access output port that does not exist for " + outNode.GetType());

                if (inNode.inputContainer.childCount < inputData.PortNumber)
                    throw new Exception("Trying to access input port that does not exist for " + inNode.GetType());

                Port outPort;
                Port inPort;

                if (outputData.PortNumber < 0)  // Invert index and use vertical instead.
                    outPort = ((Port)outNode.outputVerticalContainer[(outputData.PortNumber + 1) * -1]);
                else
                    outPort = ((Port)outNode.outputContainer[outputData.PortNumber]);


                if (inputData.PortNumber < 0)   // Invert index and use vertical instead.
                    inPort = ((Port)inNode.inputVerticalContainer[(inputData.PortNumber + 1) * -1]);
                else
                    inPort = ((Port)inNode.inputContainer[inputData.PortNumber]);


                Edge edge = outPort.ConnectTo(inPort);

                graphView.AddElement(edge);
                outNode.RefreshPorts();
            }
        }

        /// <summary>
        /// Get all serializable properties and fields in object.
        /// </summary>
        /// <param name="instance">Reference object.</param>
        /// /// <param name="binding">Property and field types.</param>
        /// <returns>Properties and fields as <see cref="JObject"/></returns>
        public static JObject GetFieldsAndProperties(object instance, BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            JObject obj = GetProperties(instance, binding);
            obj.Merge(GetFields(instance, binding));

            return obj;
        }

        /// <summary>
        /// Get all serializable fields in object.
        /// </summary>
        /// <param name="instance">Reference object.</param>
        /// /// <param name="binding">Field types.</param>
        /// <returns>Fields as <see cref="JObject"/></returns>
        public static JObject GetFields(object instance, BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            IEnumerable<FieldInfo> serializableFields = instance.GetType()
                .GetFields(binding)
                .Where(x => x.IsDefined(typeof(SerializableAttribute)) || x.IsDefined(typeof(SerializeField)));

            JObject obj = new JObject();
            foreach (FieldInfo field in serializableFields)
                obj.Add(field.Name, JToken.FromObject(field.GetValue(instance)));
            
            return obj;
        }

        /// <summary>
        /// Get all serializable properties in object.
        /// </summary>
        /// <param name="instance">Reference object.</param>
        /// /// <param name="binding">Property types.</param>
        /// <returns>Properties as <see cref="JObject"/></returns>
        public static JObject GetProperties(object instance, BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            IEnumerable<PropertyInfo> serializableFields = instance.GetType()
                .GetProperties(binding)
                .Where(x => x.IsDefined(typeof(SerializableAttribute)) || x.IsDefined(typeof(SerializeField)));

            JObject obj = new JObject();
            foreach (PropertyInfo field in serializableFields)
                obj.Add(field.Name, JToken.FromObject(field.GetValue(instance)));

            return obj;
        }

        /// <summary>
        /// Set all serializable properties and fields in object.
        /// </summary>
        /// <param name="instance">Reference object.</param>
        /// <param name="obj">Assigned data.</param>
        /// <param name="binding">Property and field types.</param>
        public static void SetFieldsAndProperties(object instance, JObject obj, BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            SetProperties(instance, obj);
            SetFields(instance, obj);
        }

        /// <summary>
        /// Set all serializable fields in object.
        /// </summary>
        /// <param name="instance">Reference object.</param>
        /// <param name="obj">Assigned data.</param>
        /// <param name="binding">Field types.</param>
        public static void SetFields(object instance, JObject obj, BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (obj == null)
                return;

            IEnumerable<FieldInfo> serializableFields = instance.GetType()
            .GetFields(binding)
            .Where(x => x.IsDefined(typeof(SerializableAttribute)) || x.IsDefined(typeof(SerializeField)));

            foreach (FieldInfo field in serializableFields)
            {
                if (obj.TryGetValue(field.Name, out JToken token))
                    field.SetValue(instance, token.ToObject(field.FieldType));
            }
        }

        /// <summary>
        /// Set all serializable properties in object.
        /// </summary>
        /// <param name="instance">Reference object.</param>
        /// <param name="obj">Assigned data.</param>
        /// <param name="binding">Property types.</param>
        public static void SetProperties(object instance, JObject obj, BindingFlags binding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (obj == null)
                return;

            IEnumerable<PropertyInfo> serializableFields = instance.GetType()
            .GetProperties(binding)
            .Where(x => x.IsDefined(typeof(SerializableAttribute)) || x.IsDefined(typeof(SerializeField)));

            foreach (PropertyInfo field in serializableFields)
            {
                if (obj.TryGetValue(field.Name, out JToken token))
                    field.SetValue(instance, token.ToObject(field.PropertyType));
            }
        }
    }
}
