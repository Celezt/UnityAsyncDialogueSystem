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
        public GUID GUID { get; private set; }

        public VisualElement inputVerticalContainer { get; private set; } = new VisualElement();
        public VisualElement outputVerticalContainer { get; private set; } = new VisualElement();

        protected DGView graphView { get; private set; }
        protected bool hasUnsavedChanges
        {
            get => graphView.EditorWindow.HasUnsavedChanges;
            set => graphView.EditorWindow.HasUnsavedChanges = value;
        }

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

            if (typeof(IPortType).IsAssignableFrom(type))
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
        internal void InternalStart(DGView graphView, GUID guid)
        {
            this.graphView = graphView;
            this.GUID = guid;

            mainContainer.Insert(0, inputVerticalContainer);
            mainContainer.Add(outputVerticalContainer);

            Awake();
        }
    }
}
