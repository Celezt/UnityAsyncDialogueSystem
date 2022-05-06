using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public abstract class NodeAsset : ScriptableObject
    {
        public IReadOnlyDictionary<string, object> NodeValues;

        public IEnumerable<NodeAsset> Inputs => _inputs;
        public IEnumerable<int> InputPortNumber => _inputPortNumbers;

        /// <summary>
        /// How many input port sockets it supports.
        /// </summary>
        public int InputCount
        {
            get => _inputCount;
            set
            {
                _inputCount = value;
                if (_inputs.Count != _inputCount)
                {
                    if (_inputCount > _inputs.Count)
                    {
                        for (int i = _inputs.Count; i < _inputCount; i++)
                        {
                            _inputs.Add(null);
                            _inputPortNumbers.Add(0);
                        }
                    }
                    else
                    {
                        for (int i = _inputs.Count; i >= _inputCount; i--)
                        {
                            _inputs.RemoveAt(i);
                            _inputPortNumbers.RemoveAt(i);
                        }
                    }
                }
            }
        }

        [SerializeField, HideInInspector]
        private List<NodeAsset> _inputs = new List<NodeAsset>();
        [SerializeField, HideInInspector]
        private List<int> _inputPortNumbers = new List<int>();

        private int _inputCount;

        /// <summary>
        /// After the asset has been deserialized. Only called when generated from graph.
        /// </summary>
        /// <param name="values"></param>
        public virtual void OnAfterDeserialize(IReadOnlyDictionary<string, object> values) { }
        /// <summary>
        /// When asset has just been created. Called after <see cref="OnAfterDeserialize"/>
        /// </summary>
        public virtual void OnCreateAsset() { }
        /// <summary>
        /// Process node behaviour based on child nodes.
        /// </summary>
        /// <param name="inputs">Child nodes.</param>
        /// <param name="outputIndex">Parent node output connection.</param>
        /// <returns>Return value.</returns>
        public abstract object Process(object[] inputs, int outputIndex);

        /// <summary>
        /// Get resulting value from this and linked nodes.
        /// </summary>
        /// <param name="portNumber"></param>
        /// <returns></returns>
        public object GetValue(int portNumber)
        {
            object[] inputs = new object[_inputs.Count];

            for (int i = 0; i < _inputs.Count; i++)
            {
                if (_inputs[i] == null)
                    continue;

                inputs[i] = _inputs[i].GetValue(_inputPortNumbers[i]);
            }

            return Process(inputs, portNumber);
        }
    }
}
