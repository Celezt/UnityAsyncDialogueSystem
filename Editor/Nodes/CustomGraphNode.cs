using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public abstract class CustomGraphNode : Node
    {
        public GUID Guid => _guid;

        public VisualElement inputVerticalContainer { get; private set; } = new VisualElement();
        public VisualElement outputVerticalContainer { get; private set; } = new VisualElement();

        protected DialogueGraphView GraphView { get; private set; }
        protected bool HasUnsavedChanges
        {
            get => GraphView.EditorWindow.HasUnsavedChanges;
            set => GraphView.EditorWindow.HasUnsavedChanges = value;
        }

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
        /// Called when created. Before load.
        /// </summary>
        protected virtual void Awake() { }
        /// <summary>
        /// Called after loading data from file. Will always be called.
        /// </summary>
        protected virtual void Start() { }
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
        /// <summary>
        /// Data to save connected to this node.
        /// </summary>
        protected virtual object OnSerialization() { return null; }
        /// <summary>
        /// Called when data is loaded from file. Not called if save returns null.
        /// </summary>
        /// <param name="loadedData">Loaded data</param>
        protected virtual void OnDeserialization(JObject loadedData) { }

        internal object InternalGetSaveData() => OnSerialization();
        internal void InternalAfterLoad() => Start();
        internal void InternalInvokeEdgeChange(Edge edge, EdgeState state) => OnEdgeChanged(edge, state);
        internal void InternalInvokeDestroy() => OnDestroy();
        internal void InternalSetLoadData(JObject loadedData) => OnDeserialization(loadedData);
        internal void InternalStart(DialogueGraphView graphView, GUID guid)
        {
            GraphView = graphView;
            _guid = guid;

            mainContainer.Insert(0, inputVerticalContainer);
            mainContainer.Add(outputVerticalContainer);

            Awake();
        }
    }
}
