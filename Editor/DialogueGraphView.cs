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

    public class DialogueGraphView : GraphView
    {
        private DialogueEditorWindow _editorWindow;
        private NodeSearchWindow _searchWindow;

        public DialogueGraphView(DialogueEditorWindow editorWindow)
        {
            _editorWindow = editorWindow;

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddStyles();
        }

        public T CreateNode<T>(Vector2 position) where T : Node, INode, new()
        {
            T node = new T();
            node.Initialize(position);
            node.Draw();

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
            this.AddManipulator(CreateNodeContextualMenu<ParagraphNode>("Paragraph Node"));
        }

        private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : Node, INode, new() => new ContextualMenuManipulator(
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
                _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
                _searchWindow.Initialize(this, _editorWindow);
            }

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DialogueGraphViewStyles",
                "DialogueNodeStyles"
            );
        }
    }
}
