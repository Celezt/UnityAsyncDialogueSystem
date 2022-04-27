using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    /// <summary>
    /// Main DialogueGraphView node class.
    /// </summary>
    public abstract class DGNode : Node
    {
        public Guid ID { get; private set; }

        /// <summary>
        /// Vertical input container used for vertical input ports.
        /// </summary>
        public VisualElement inputVerticalContainer { get; } = new VisualElement()
        {
            name = "input-vertical",
        };
        /// <summary>
        /// Vertical output container used for vertical output ports.
        /// </summary>
        public VisualElement outputVerticalContainer { get; } = new VisualElement()
        {
            name = "output-vertical",
        };
        /// <summary>
        /// Control container used for controllable content.
        /// </summary>
        public VisualElement controlContainer { get; } = new VisualElement()
        {
            name = "controls",
        };

        protected DGView graphView { get; private set; }
        protected bool hasUnsavedChanges
        {
            get => graphView.EditorWindow.hasUnsavedChanges;
            set => graphView.EditorWindow.hasUnsavedChanges = value;
        }

        private VisualElement _parentControlContainer = new VisualElement()
        {
            name = "bottom",
        };

        [Flags]
        public enum EdgeState
        {
            Created = 1 << 1,
            Removed = 1 << 2,
            Input = 1 << 3,
            Output = 1 << 4,
        }

        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            Port port = base.InstantiatePort(orientation, direction, capacity, type);
            port.portName = "";

            if (!type.IsInterface && typeof(IPortType).IsAssignableFrom(type))
            {
                IPortType portType = (IPortType)Activator.CreateInstance(type);
                port.portColor = portType.Color;
            }

            return port;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Group Selection", actionEvent =>
            {
                graphView.AddElement(graphView.CreateGroup(graphView.GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)));
            });

            base.BuildContextualMenu(evt);
        }

        /// <summary>
        /// Called when created.
        /// </summary>
        protected virtual void Awake() { }
        /// <summary>
        /// Called if the state of any edges connected to this node is about be to changed.
        /// </summary>
        /// <param name="edge">The changing edge.</param>
        /// <param name="state">The direction and life state.</param>
        protected virtual void OnEdgeChanged(Edge edge, EdgeState state) { }
        /// <summary>
        /// Called when node is about to be destroyed.
        /// </summary>
        protected virtual void OnDestroy() { }

        internal void InternalInvokeEdgeChange(Edge edge, EdgeState state) => OnEdgeChanged(edge, state);
        internal void InternalInvokeDestroy() => OnDestroy();
        internal void InternalInitialize(DGView graphView, Guid id)
        {
            this.graphView = graphView;
            this.ID = id;

            mainContainer.Insert(0, inputVerticalContainer);
            topContainer.parent.Add(_parentControlContainer);

            var controlDivider = new VisualElement
            {
                name = "divider",
            };
            controlDivider.AddToClassList("horizontal");
            _parentControlContainer.Add(controlDivider);
            _parentControlContainer.Add(controlContainer);

            var outputVerticalDivider = new VisualElement
            {
                name = "divider",
            };
            outputVerticalDivider.AddToClassList("horizontal");
            outputVerticalContainer.Add(outputVerticalDivider);
            mainContainer.Add(outputVerticalContainer);

            Awake();

            if (controlContainer.childCount <= 0)   // If control container is empty.
            {
                controlContainer.RemoveFromHierarchy();
            }
        }
    }
}
