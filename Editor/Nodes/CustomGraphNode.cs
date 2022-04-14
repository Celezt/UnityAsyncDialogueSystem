using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class CustomGraphNode : Node
    {
        public GUID Guid => _guid;

        protected GraphView GraphView { get; private set; }

        private GUID _guid;

        [Flags]
        public enum EdgeState
        {
            Created = 1 << 1,
            Removed = 1 << 2,
            Input = 1 << 3,
            Output = 1 << 4,
        }

        /// <summary>
        /// Called when created.
        /// </summary>
        protected virtual void Start() { }
        /// <summary>
        /// Called after loading data from file. Will always be called.
        /// </summary>
        protected virtual void AfterLoad() { }
        /// <summary>
        /// Called if the state of any edges connected to this node is about to changed.
        /// </summary>
        /// <param name="edge">The changing edge.</param>
        /// <param name="state">The direction and life state.</param>
        protected virtual void OnEdgeChanged(Edge edge, EdgeState state) { }
        /// <summary>
        /// Called when node is about to be destroyed.
        /// </summary>
        protected virtual void OnDestroy() { }
        /// <summary>
        /// Data to save connected to this node.
        /// </summary>
        protected virtual object OnSaveData() { return null; }
        /// <summary>
        /// Called when data is loaded from file. Not called if save returns null.
        /// </summary>
        /// <param name="loadedData">Loaded data</param>
        protected virtual void OnLoadData(JObject loadedData) { }

        internal object InternalGetSaveData() => OnSaveData();
        internal void InternalAfterLoad() => AfterLoad();
        internal void InternalInvokeEdgeChange(Edge edge, EdgeState state) => OnEdgeChanged(edge, state);
        internal void InternalInvokeDestroy() => OnDestroy();
        internal void InternalSetLoadData(JObject loadedData) => OnLoadData(loadedData);
        internal void InternalStart(GraphView graphView, GUID guid)
        {
            GraphView = graphView;
            _guid = guid;

            Start();
        }
    }
}
