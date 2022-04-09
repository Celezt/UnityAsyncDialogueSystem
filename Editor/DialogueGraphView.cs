using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace Celezt.DialogueSystem.Editor
{
    using Utilities;
    using DialogueSystem.Utilities;

    public class DialogueGraphView : GraphView
    {
        private DialogueGraphEditorWindow _editorWindow;
        private DialogueGraphNodeSearchWindow _searchWindow;

        public DialogueGraphView(DialogueGraphEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

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
                if (selectedElement is DSNode node)
                    group.AddElement(node);

            return group;
        }

        public T CreateNode<T>(Vector2 position) where T : DSNode
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

        private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : DSNode, new() => new ContextualMenuManipulator(
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
                        if (edge.input.node is DSNode inNode)
                            inNode.InvokeEdgeChange(edge, DSNode.EdgeState.Created | DSNode.EdgeState.Input);
                        if (edge.output.node is DSNode outNode)
                            outNode.InvokeEdgeChange(edge, DSNode.EdgeState.Created | DSNode.EdgeState.Output);
                    }
                }

                if (changes.elementsToRemove is { })
                {
                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element is Edge edge)
                        {
                            if (edge.input.node is DSNode inNode)
                                inNode.InvokeEdgeChange(edge, DSNode.EdgeState.Removed | DSNode.EdgeState.Input);
                            if (edge.output.node is DSNode outNode)
                                outNode.InvokeEdgeChange(edge, DSNode.EdgeState.Removed | DSNode.EdgeState.Output);
                        }                    
                    }
                }

                return changes;
            };
        }
    }
}
