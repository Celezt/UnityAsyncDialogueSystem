using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace Celezt.DialogueSystem.Editor
{
    using Unity.Plastic.Newtonsoft.Json.Linq;
    using Utilities;

    public class DialogueGraphView : GraphView
    {
        internal const int DG_VERSION = 1;

        internal DialogueGraphEditorWindow EditorWindow
        {
            get => _editorWindow;
            private set => _editorWindow = value;
        }

        [NonSerialized]
        internal Dictionary<Type, NodeTypeData> NodeTypes = new Dictionary<Type, NodeTypeData>();
        [NonSerialized]
        internal Dictionary<GUID, CustomGraphNode> NodeDictionary = new Dictionary<GUID, CustomGraphNode>();

        private DialogueGraphEditorWindow _editorWindow;
        private DialogueGraphNodeSearchWindow _searchWindow;


        internal struct NodeTypeData
        {
            internal string MenuName;
        }

        public DialogueGraphView(DialogueGraphEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            ReflectNodes();
            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            OnGraphViewChanged();
            AddStyles();
            
            canPasteSerializedData += AllowPaste;
            unserializeAndPaste += OnPaste;
        }

        public GraphElement CreateGroup(Vector2 position)
        {
            Group group = new Group()
            {
                title = "New Group",
            };

            group.SetPosition(new Rect(position, Vector2.zero));

            foreach (GraphElement selectedElement in selection)
                if (selectedElement is CustomGraphNode node)
                    group.AddElement(node);

            return group;
        }

        public CustomGraphNode CopyNode(CustomGraphNode toCopy, Vector2 position)
        {
            var node = CreateNode(toCopy.GetType(), position, GUID.Generate(), SerializationUtility.ToJObject(toCopy.InternalGetSaveData()));

            return node;
        }

        public CustomGraphNode CreateNode(Type type, Vector2 position, GUID guid, JObject loadedData = null)
        {
            if (!typeof(CustomGraphNode).IsAssignableFrom(type))
            {
                Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(CustomGraphNode)}");
                return null;
            }

            var node = (CustomGraphNode)Activator.CreateInstance(type);
            node.SetPosition(new Rect(position, Vector2.zero));
            node.InternalStart(this, guid);
            if (loadedData != null)
                node.InternalSetLoadData(loadedData);
            node.InternalAfterLoad();
            NodeDictionary.Add(guid, node);

            return node;
        }

        public T CreateNode<T>(Vector2 position, GUID guid, JObject loadedData = null) where T : CustomGraphNode, new()
        {
            T node = new T();
            node.SetPosition(new Rect(position, Vector2.zero));
            node.InternalStart(this, guid);
            if (loadedData != null)
                node.InternalSetLoadData(loadedData);
            node.InternalAfterLoad();
            NodeDictionary.Add(guid, node);

            return node;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort.node == port.node)            // Ignore if same node.
                    return;

                if (startPort.direction == port.direction)  // Ignore if same direction (input/output).
                    return;

                if (startPort.portType != port.portType)    // Ignore if not same port type.
                    return;

                compatiblePorts.Add(port);
            });
            return compatiblePorts;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition) => contentViewContainer.WorldToLocal(mousePosition);

        /// <summary>
        /// Reflect all dialogue graph nodes.
        /// </summary>
        private void ReflectNodes()
        {
            foreach (Type type in ReflectionUtility.GetTypesWithAttribute<CreateNodeAttribute>(AppDomain.CurrentDomain))
            {
                if (!typeof(CustomGraphNode).IsAssignableFrom(type))
                {
                    Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(CustomGraphNode)}");
                    continue;
                }

                CreateNodeAttribute createNodeAttribute = type.GetCustomAttribute<CreateNodeAttribute>();
                NodeTypes.Add(type, new NodeTypeData { MenuName = createNodeAttribute.MenuName });
            }
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateGroupContextualMenu() => new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Create Group", actionEvent => AddElement(CreateGroup(GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));

        private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : CustomGraphNode, new() => new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Create " + actionTitle, actionEvent => AddElement(CreateNode<T>(GetLocalMousePosition(actionEvent.eventInfo.localMousePosition), GUID.Generate()))));

        private void AddGridBackground()
        {     
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (_searchWindow is null)
            {
                _searchWindow = ScriptableObject.CreateInstance<DialogueGraphNodeSearchWindow>();
                _searchWindow.Initialize(this, _editorWindow);
            }
            
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddStyles()
        {
            this.AddStyleSheet("DGView");
            this.AddStyleSheet("DGNode");
        }

        private void OnGraphViewChanged() 
        {         
            
            graphViewChanged = changes =>
            {
                if (changes.edgesToCreate is { })
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        if (edge.input.node is CustomGraphNode inNode)
                            inNode.InternalInvokeEdgeChange(edge, CustomGraphNode.EdgeState.Created | CustomGraphNode.EdgeState.Input);
                        if (edge.output.node is CustomGraphNode outNode)
                            outNode.InternalInvokeEdgeChange(edge, CustomGraphNode.EdgeState.Created | CustomGraphNode.EdgeState.Output);
                    }
                }
                
                if (changes.elementsToRemove is { })
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            if (edge.input.node is CustomGraphNode inNode)
                                inNode.InternalInvokeEdgeChange(edge, CustomGraphNode.EdgeState.Removed | CustomGraphNode.EdgeState.Input);
                            if (edge.output.node is CustomGraphNode outNode)
                                outNode.InternalInvokeEdgeChange(edge, CustomGraphNode.EdgeState.Removed | CustomGraphNode.EdgeState.Output);
                        }   
                        
                        if (element is Node node)
                        {
                            if (node is CustomGraphNode customNode)
                            {
                                customNode.InternalInvokeDestroy();
                                NodeDictionary.Remove(customNode.Guid);
                            }
                        }
                    }
                }

                return changes;
            };

            deleteSelection = (operationName, askUser) =>
            {
                List<Edge> edgesToDelete = new List<Edge>();
                List<Node> nodesToDelete = new List<Node>();
                List<Group> groupsToDelete = new List<Group>();
                foreach (GraphElement element in selection)
                {
                    switch (element)
                    {
                        case Node node:
                            {
                                nodesToDelete.Add(node);
                            }
                            break;
                        case Edge edge:
                            {
                                edgesToDelete.Add(edge);
                            }
                            break;
                        case Group group:
                            {
                                groupsToDelete.Add(group);
                            }
                            break;
                        default:
                            break;
                    }
                }

                foreach (Node node in nodesToDelete)
                {
                    if (node.inputContainer.childCount > 0)
                    {
                        foreach (Port port in node.inputContainer.Children().OfType<Port>())
                            DeleteElements(port.connections);
                    }

                    if (node.outputContainer.childCount > 0)
                    {
                        foreach (Port port in node.outputContainer.Children().OfType<Port>())
                            DeleteElements(port.connections);
                    }

                    if (node is CustomGraphNode customNode)
                    {
                        if (customNode.inputVerticalContainer.childCount > 0)
                        {
                            foreach (Port port in customNode.inputVerticalContainer.Children().OfType<Port>())
                                DeleteElements(port.connections);
                        }

                        if (customNode.outputVerticalContainer.childCount > 0)
                        {
                            foreach (Port port in customNode.outputVerticalContainer.Children().OfType<Port>())
                                DeleteElements(port.connections);
                        }
                    }
                }

                DeleteElements(edgesToDelete);
                DeleteElements(groupsToDelete);
                DeleteElements(nodesToDelete);
            };
        }

        private void OnPaste(string operationName, string data)
        {
            if (selection.Count > 0)
            {
                foreach (ISelectable selectable in selection)
                {
                    Vector2 centerPosition = _editorWindow.position.center;
                    Vector2 worldMouePosition = _editorWindow.rootVisualElement.ChangeCoordinatesTo(_editorWindow.rootVisualElement.parent, centerPosition - _editorWindow.position.position);
                    Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMouePosition);

                    if (selectable is CustomGraphNode node)
                    {
                        var newNode = CopyNode(node, node.GetPosition().position + new Vector2(100, 50));
                        AddElement(newNode);
                    }
                }
            }
        }

        private bool AllowPaste(string data)
        {
            return true;
        }
    }
}
