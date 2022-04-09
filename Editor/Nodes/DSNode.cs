using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class DSNode : Node
    {
        public Guid ID => Guid.NewGuid();

        protected GraphView GraphView { get; private set; }

        [Flags]
        public enum EdgeState
        {
            Created = 1 << 1,
            Removed = 1 << 2,
            Input = 1 << 3,
            Output = 1 << 4,
        }

        protected DSNode(GraphView graphView, Vector2 position)
        {
            SetPosition(new Rect(position, Vector2.zero));
            GraphView = graphView;
        }

        protected virtual void OnEdgeChanged(Edge edge, EdgeState state) { }

        internal void InvokeEdgeChange(Edge edge, EdgeState state) => OnEdgeChanged(edge, state);
    }
}
