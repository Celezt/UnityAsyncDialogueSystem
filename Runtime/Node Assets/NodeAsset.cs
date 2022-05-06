using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public abstract class NodeAsset : ScriptableObject
    {
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
        private bool _initialized;

        /// <summary>
        /// When asset has just been created. 
        /// </summary>
        /// <param name="values">Deserialized values.</param>
        protected virtual void OnCreateAsset(IReadOnlyDictionary<string, object> values) { }
        /// <summary>
        /// Process node behaviour based on child nodes.
        /// </summary>
        /// <param name="inputs">Child nodes.</param>
        /// <param name="outputIndex">Parent node output connection.</param>
        /// <returns>Return value.</returns>
        protected abstract object Process(object[] inputs, int outputIndex);

        /// <summary>
        /// Get resulting value from this and connected nodes.
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

        /// <summary>
        /// Initialize this and connected children.
        /// </summary>
        public void InitializeTree()
        {
            for (int i = 0; i < _inputs.Count; i++)
            {
                if (_inputs[i] == null)
                    continue;

                _inputs[i].InitializeTree();
            }

            if (_initialized == false)
                OnCreateAsset(null);

            _initialized = true;
        }
    }
}
