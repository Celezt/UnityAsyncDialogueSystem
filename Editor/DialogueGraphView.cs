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
        internal Dictionary<Type, NodeTypeData> NodeTypes = new Dictionary<Type, NodeTypeData>();

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
                if (selectedElement is DialogueGraphNode node)
                    group.AddElement(node);

            return group;
        }

        public DialogueGraphNode CreateNode(Type type, Vector2 position)
        {
            if (!typeof(DialogueGraphNode).IsAssignableFrom(type))
            {
                Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(DialogueGraphNode)}");
                return null;
            }

            return (DialogueGraphNode)Activator.CreateInstance(type, this, position);
        }

        public T CreateNode<T>(Vector2 position) where T : DialogueGraphNode
        {
            T node = (T)Activator.CreateInstance(typeof(T), this, position);

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
                if (!typeof(DialogueGraphNode).IsAssignableFrom(type))
                {
                    Debug.LogError($"DIALOGUE ERROR: {type} has no derived {nameof(DialogueGraphNode)}");
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

        private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : DialogueGraphNode, new() => new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Create " + actionTitle, actionEvent => AddElement(CreateNode<T>(GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))));

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
                        if (edge.input.node is DialogueGraphNode inNode)
                            inNode.InvokeEdgeChange(edge, DialogueGraphNode.EdgeState.Created | DialogueGraphNode.EdgeState.Input);
                        if (edge.output.node is DialogueGraphNode outNode)
                            outNode.InvokeEdgeChange(edge, DialogueGraphNode.EdgeState.Created | DialogueGraphNode.EdgeState.Output);
                    }
                }

                if (changes.elementsToRemove is { })
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            if (edge.input.node is DialogueGraphNode inNode)
                                inNode.InvokeEdgeChange(edge, DialogueGraphNode.EdgeState.Removed | DialogueGraphNode.EdgeState.Input);
                            if (edge.output.node is DialogueGraphNode outNode)
                                outNode.InvokeEdgeChange(edge, DialogueGraphNode.EdgeState.Removed | DialogueGraphNode.EdgeState.Output);
                        }                    
                    }
                }

                return changes;
            };
        }
    }
}
