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

        /// <summary>
        /// If the state of any edges connected to this node is about to changed.
        /// </summary>
        /// <param name="edge">The changing edge.</param>
        /// <param name="state">The direction and life state.</param>
        protected virtual void OnEdgeChanged(Edge edge, EdgeState state) { }
        /// <summary>
        /// Custom data to save connected to this node.
        /// </summary>
        protected virtual object CustomSaveData() { return null; }
        /// <summary>
        /// Custom data loaded from file.
        /// </summary>
        /// <param name="loadedData">Loaded data</param>
        protected virtual void CustomLoadData(object loadedData) { }

        internal void InvokeEdgeChange(Edge edge, EdgeState state) => OnEdgeChanged(edge, state);
        internal object GetCustomSaveData() => CustomSaveData();
    }
}
