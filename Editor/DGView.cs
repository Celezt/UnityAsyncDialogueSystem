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
using Newtonsoft.Json.Linq;

namespace Celezt.DialogueSystem.Editor
{
    public class DGView : GraphView
    {
        internal const int DG_VERSION = 1;

        internal DGEditorWindow EditorWindow
        {
            get => _editorWindow;
            private set => _editorWindow = value;
        }

        internal DGBlackboard Blackboard
        {
            get => _blackboard;
            private set => _blackboard = value;
        }

        internal Dictionary<Type, NodeTraits> NodeTypeDictionary { get; private set; } = new Dictionary<Type, NodeTraits>()
        {
            {typeof(PropertyNode), new NodeTraits{ AssetBinder = typeof(ValueProcessor) } },
            {typeof(BasicNode), new NodeTraits{ AssetBinder = typeof(ValueProcessor) } },
        };
        internal Dictionary<Guid, DGNode> NodeDictionary { get; private set; } = new Dictionary<Guid, DGNode>();

        private DGEditorWindow _editorWindow;
        private DGNodeSearchWindow _searchWindow;
        private DGBlackboard _blackboard;

        public DGView(DGEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            ReflectNodes();
            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddBlackboard();
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
                if (selectedElement is DGNode node)
                    group.AddElement(node);

            return group;
        }

        public DGNode CopyNode(DGNode toCopy, Vector2 position)
        {
            var node = CreateNode(toCopy.GetType(), position, Guid.NewGuid(), JsonUtility.GetFields(toCopy), toCopy.userData);

            return node;
        }

        public DGNode CreateNode(Type type, Vector2 position, Guid id, JObject obj = null, object userData = null)
        {
            if (!typeof(DGNode).IsAssignableFrom(type))
            {
                Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(DGNode)}");
                return null;
            }

            var node = (DGNode)Activator.CreateInstance(type);
            JsonUtility.SetFields(node, obj);
           
            if (NodeTypeDictionary.TryGetValue(type, out NodeTraits properties))
            {
                if (!string.IsNullOrWhiteSpace(properties.NodeTitle))
                    node.title = properties.NodeTitle;
            }

            node.userData = userData;
            node.SetPosition(new Rect(position, Vector2.zero));
            node.InternalInitialize(this, id);
            NodeDictionary.Add(id, node);

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

                if (!(port.portType.IsAssignableFrom(startPort.portType) || startPort.portType.IsAssignableFrom(port.portType)))    // Ignore if not same port type.
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
            foreach (Type nodeType in ReflectionUtility.GetTypesWithAttribute<CreateNodeAttribute>(AppDomain.CurrentDomain))
            {
                if (!typeof(DGNode).IsAssignableFrom(nodeType))
                {
                    Debug.LogError($"DIALOGUE ERROR: {nodeType} has no derived {nameof(DGNode)}");
                    continue;
                }
                
                CreateNodeAttribute createNodeAttribute = nodeType.GetCustomAttribute<CreateNodeAttribute>();
                AssetBinderAttribute assetBinderAttribute = nodeType.GetCustomAttribute<AssetBinderAttribute>();

                NodeTypeDictionary.Add(nodeType, new NodeTraits 
                { 
                    MenuName = createNodeAttribute.MenuName,
                    NodeTitle = createNodeAttribute.NodeTitle,
                    AssetBinder = assetBinderAttribute?.AssetBinder ?? null,
                });
            }
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new RectangleSelector());
        }
       
        private void AddGridBackground()
        {     
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (_searchWindow == null)
            {
                _searchWindow = ScriptableObject.CreateInstance<DGNodeSearchWindow>();
                _searchWindow.Initialize(this, _editorWindow);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddBlackboard()
        {
            if (_blackboard == null)
            {
                _blackboard = new DGBlackboard(this);
                Add(_blackboard);
            }
            _editorWindow.OnTitleChanged += newTitle =>
            {
                _blackboard.title = newTitle;
            };
        }

        private void AddStyles()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "DGView");
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "DGNode");
        }

        private void OnGraphViewChanged() 
        {         
            graphViewChanged = changes =>
            {
                if (changes.edgesToCreate is { })
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        if (edge.input.node is DGNode inNode)
                            inNode.InternalInvokeEdgeChange(edge, DGNode.EdgeState.Created | DGNode.EdgeState.Input);
                        if (edge.output.node is DGNode outNode)
                            outNode.InternalInvokeEdgeChange(edge, DGNode.EdgeState.Created | DGNode.EdgeState.Output);
                    }
                }
                
                if (changes.elementsToRemove is { })
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            if (edge.input != null && edge.input.node is DGNode inNode)
                                inNode.InternalInvokeEdgeChange(edge, DGNode.EdgeState.Removed | DGNode.EdgeState.Input);
                            if (edge.output != null && edge.output.node is DGNode outNode)
                                outNode.InternalInvokeEdgeChange(edge, DGNode.EdgeState.Removed | DGNode.EdgeState.Output);
                        }   
                        
                        if (element is Node node)
                        {
                            if (node is DGNode customNode)
                            {
                                customNode.InternalInvokeDestroy();
                                NodeDictionary.Remove(customNode.ID);
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

                    if (node is DGNode customNode)
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

                    if (selectable is DGNode node)
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
