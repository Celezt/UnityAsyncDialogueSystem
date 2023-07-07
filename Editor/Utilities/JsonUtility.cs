using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Celezt.DialogueSystem.Editor
{
    public static class JsonUtility
    {
        public static JObject ToJObject(object convert)
        {
            return JObject.FromObject(convert);
        }

        public static string Serialize(object serialize)
        {
            return JsonConvert.SerializeObject(serialize);
        }

        public static object Deserialize(string deserialize, Type type)
        {
            return JsonConvert.DeserializeObject(deserialize.ToString(), type);
        }

        public static string SerializeGraph(int version, GUID objectID, DGView graphView)
        {
            List<NodeSerialized> nodeData = new List<NodeSerialized>();
            List<EdgeSerialized> edgeData = new List<EdgeSerialized>();
            List<SerializedVector2Int> positionData = new List<SerializedVector2Int>();
            List<object> specialData = new List<object>();
            List<object> propertyData = new List<object>();

            foreach (var property in graphView.Blackboard.Properties)
            {
                JObject obj = new JObject();
                obj.Add("ID", property.ID.ToString("N"));               // Unique ID.
                obj.Add("Type", property.ValueType.FullName);           // Value type.
                obj.Add("Name", property.Name);                         // Property name.
                obj.Add("Value", JToken.FromObject(property.Value));    // Value.
                propertyData.Add(obj);
            }

            graphView.nodes.ForEach(node =>
            {            
                if (node is DGNode { } dgNode)
                {
                    positionData.Add(dgNode.GetPosition().position);
                    specialData.Add(GetFields(dgNode));

                    nodeData.Add(new NodeSerialized
                    {
                        ID = dgNode.ID.ToString("N"),
                        Type = dgNode.GetType().FullName,
                        Binder = graphView.NodeTypeDictionary[dgNode.GetType()].AssetBinder?.FullName ?? "",
                    });;
                }
            });

            graphView.edges.ForEach(edge =>
            {
                if (edge.input.node is DGNode inNode)
                {
                    if (edge.output.node is DGNode outNode)
                    {
                        edgeData.Add(new EdgeSerialized
                        {
                            InputPort =
                            {
                                NodeID = inNode.ID.ToString("N"),
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
                                NodeID = outNode.ID.ToString("N"),
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

            GraphSerialized graphSerializeData = new GraphSerialized
            {
                DGVersion = version,
                ObjectID = objectID.ToString(),
                Properties = propertyData,
                Nodes = nodeData,
                Edges = edgeData,
                Positions = positionData,
                Data = specialData,
            };


            return JsonConvert.SerializeObject(graphSerializeData, Formatting.Indented);
        }

        public static string SerializeGraph(int version, GUID objectID)
        {
            GraphSerialized graphSerializeData = new GraphSerialized
            {
                DGVersion = version,
                ObjectID = objectID.ToString(),
                Properties = new List<dynamic>(),
                Nodes = new List<NodeSerialized>(),
                Edges = new List<EdgeSerialized>(),
                Positions = new List<SerializedVector2Int>(),
                Data = new List<dynamic>(),
            };


            return JsonConvert.SerializeObject(graphSerializeData, Formatting.Indented);
        }

        public static GraphSerialized DeserializeGraph(string content)
        {
            return JsonConvert.DeserializeObject<GraphSerialized>(content.ToString());
        }

        internal static void DeserializeGraph(this DGView graphView, string content)
        {
            GraphSerialized deserializedData = DeserializeGraph(content);

            if (deserializedData.Properties != null)
            {
                //
                // Deserialize all properties.
                //
                foreach (object obj in deserializedData.Properties)
                {
                    JObject jObj = (JObject)obj;

                    Guid id = Guid.Empty;
                    {
                        if (jObj.TryGetValue("ID", out JToken token))
                        {
                            string guidString = token.ToObject<string>();
                            if (!Guid.TryParseExact(guidString, "N", out id))
                            {
                                Debug.LogWarning("Unable to parse Guid: " + guidString);
                                continue;
                            }

                        }
                    }

                    if (id == Guid.Empty)
                    {
                        Debug.LogWarning("Unable to find blackboard property \"ID\"");
                        continue;
                    }

                    Type valueType = null;
                    {
                        if (jObj.TryGetValue("Type", out JToken token))
                            valueType = Type.GetType(token.ToObject<string>());
                    }

                    if (valueType == null)
                    {
                        Debug.LogWarning("Unable to find blackboard property \"Type\"");
                        continue;
                    }

                    Type propertyType = graphView.Blackboard.GetPropertyType(valueType);
                    if (propertyType == null)
                    {
                        Debug.LogWarning("Property value does not exist for: " + valueType.FullName);
                        continue;
                    }
                        
                    IBlackboardProperty property = (IBlackboardProperty)Activator.CreateInstance(propertyType);
                    property.SetID(id);

                    {
                        if (jObj.TryGetValue("Name", out JToken token))
                        {
                            property.Name = token.ToObject<string>();
                        }
                    }
                    {
                        if (jObj.TryGetValue("Value", out JToken token))
                        {
                            property.Value = token.ToObject(property.ValueType);
                        }
                    }

                    graphView.Blackboard.AddProperty(property);
                }
            }        

            //
            // Deserialize all nodes.
            //
            for (int i = 0; i < deserializedData.Nodes.Count; i++)
            {
                NodeSerialized nodeData = deserializedData.Nodes[i];
                SerializedVector2Int positionData = deserializedData.Positions[i];
                JObject specialData = deserializedData.Data[i] as JObject;

                if (!Guid.TryParseExact(nodeData.ID, "N", out Guid id))
                    throw new Exception(nodeData.ID + " is invalid GUID");

                Type deserializedType = Type.GetType(nodeData.Type);
                object userData = null;

                //
                // If property node.
                //
                if (deserializedType == typeof(PropertyNode))    
                {
                    if (specialData.TryGetValue("_propertyID", out JToken token))   // Get property id from property node.
                    {
                        if (Guid.TryParseExact(token.ToObject<string>(), "N", out Guid propertyID))
                        {
                            foreach (var currentProperty in graphView.Blackboard.Properties)
                            {
                                if (currentProperty.ID == propertyID)
                                {
                                    userData = currentProperty;
                                    break;
                                }
                            }
                        }
                    }
                }

                //
                // If basic node.
                //
                if (deserializedType == typeof(BasicNode))
                {
                    if (specialData.TryGetValue("_type", out JToken token))
                    {
                        userData = graphView.Blackboard.GetPropertyType(Type.GetType(token.ToObject<string>()));
                    }
                }

                DGNode graphNode = graphView.CreateNode(deserializedType, positionData, id, specialData, userData);
                graphView.AddElement(graphNode);
            }

            //
            //  Deserialize all edges.
            //
            for (int i = 0; i < deserializedData.Edges.Count; i++)
            {
                EdgeSerialized edgeData = deserializedData.Edges[i];
                PortSerialized outputData = edgeData.OutputPort;
                PortSerialized inputData = edgeData.InputPort;

                if (!Guid.TryParseExact(outputData.NodeID, "N", out Guid outguid))
                    throw new Exception(outputData.NodeID + " is invalid GUID");

                if (!Guid.TryParseExact(inputData.NodeID, "N", out Guid inguid))
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
