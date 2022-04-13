using System;
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
    using Utilities;

    public class DialogueGraphView : GraphView
    {
        internal const int DG_VERSION = 1;
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

        public CustomGraphNode CreateNode(Type type, Vector2 position, GUID guid)
        {
            if (!typeof(CustomGraphNode).IsAssignableFrom(type))
            {
                Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(CustomGraphNode)}");
                return null;
            }

            var node = (CustomGraphNode)Activator.CreateInstance(type);
            node.SetPosition(new Rect(position, Vector2.zero));
            node.InternalStart(this, guid);
            NodeDictionary.Add(guid, node);

            return node;
        }

        public T CreateNode<T>(Vector2 position, GUID guid) where T : CustomGraphNode, new()
        {
            T node = new T();
            node.SetPosition(new Rect(position, Vector2.zero));
            node.InternalStart(this, guid);
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
            this.AddStyleSheet("DSGraphViewStyles");
            this.AddStyleSheet("DSNodeStyles");
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
        }
    }
}
