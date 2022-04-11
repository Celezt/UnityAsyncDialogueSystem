using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class DialogueGraphNode : Node
    {
        public GUID ID => _id;

        private readonly GUID _id = GUID.Generate();

        protected GraphView GraphView { get; private set; }

        [Flags]
        public enum EdgeState
        {
            Created = 1 << 1,
            Removed = 1 << 2,
            Input = 1 << 3,
            Output = 1 << 4,
        }

        protected DialogueGraphNode(GraphView graphView, Vector2 position)
        {
            SetPosition(new Rect(position, Vector2.zero));
            GraphView = graphView;
        }

        protected virtual void OnEdgeChanged(Edge edge, EdgeState state) { }

        internal void InvokeEdgeChange(Edge edge, EdgeState state) => OnEdgeChanged(edge, state);
    }
}
