using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;

namespace Celezt.DialogueSystem.Editor
{
    public class DialogueGraphView : GraphView
    {
        public const string STYLE_PATH = "Packages/com.celezt.asyncdialogue/Editor/Resources/Styles/";

        public DialogueGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
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

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateNodeContextualMenu<DialogueNode>("Dialogue Node"));
        }

        private IManipulator CreateNodeContextualMenu<T>(string actionTitle) where T : Node, INode, new() => new ContextualMenuManipulator(
            menuEvent => menuEvent.menu.AppendAction("Create " + actionTitle, actionEvent => AddElement(CreateNode<T>(actionEvent.eventInfo.localMousePosition))));

        private void AddGridBackground()
        {     
            GridBackground gridBackground = new GridBackground();
            gridBackground.StretchToParentSize();
            Insert(0, gridBackground);
        }

        private T CreateNode<T>(Vector2 position) where T : Node, INode, new()
        {
            T node = new T();
            node.Initialize(position);
            node.Draw();

            return node;
        }

        private void AddStyles()
        {
            CreateStyle("DialogueGraphViewStyles");
            CreateStyle("DialogueNodeStyles");
        }

        private void CreateStyle(string styleName) => 
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(STYLE_PATH + styleName + ".uss"));
    }
}
