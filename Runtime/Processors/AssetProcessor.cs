using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Celezt.DialogueSystem
{
    /// <summary>
    /// Process logic connected to behaviours.
    /// </summary>
    public abstract class AssetProcessor : ScriptableObject, IDSAsset
    {
        public IEnumerable<AssetProcessor> Inputs => _inputs;
        public IEnumerable<int> OutputPortNumbers => _outputPortNumbers;

        /// <summary>
        /// How many input port sockets it supports.
        /// </summary>
        public virtual int InputCount => 0;
        
        [HideInInspector]
        public UnityEvent OnChanged = new UnityEvent();

        [SerializeField, HideInInspector]
        internal List<AssetProcessor> _inputs = new List<AssetProcessor>();
        [SerializeField, HideInInspector]
        internal List<int> _outputPortNumbers = new List<int>();

        internal DSNode _node;

        private bool _initialized;

        /// <summary>
        /// If asset has any changes.
        /// </summary>
        public void IsDirty()
        {
            OnChanged.Invoke();
        }

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

                inputs[i] = _inputs[i].GetValue(_outputPortNumbers[i]);
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

                _inputs[i].OnChanged.RemoveListener(IsDirty);
                _inputs[i].OnChanged.AddListener(IsDirty);

                _inputs[i].InitializeTree();
            }

            if (_initialized == false)
                OnCreateAsset(_node?.Values ?? new Dictionary<string, object>());

            _initialized = true;
        }

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

        private void Awake()
        {
            if (_inputs.Count != InputCount)
            {
                if (InputCount > _inputs.Count)
                {
                    for (int i = _inputs.Count; i < InputCount; i++)
                    {
                        _inputs.Add(null);
                        _outputPortNumbers.Add(0);
                    }
                }
                else
                {
                    for (int i = _inputs.Count; i >= InputCount; i--)
                    {
                        _inputs.RemoveAt(i);
                        _outputPortNumbers.RemoveAt(i);
                    }

                }
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _inputs.Count; i++)
            {
                if (_inputs[i] == null)
                    continue;

                _inputs[i].OnChanged.RemoveListener(IsDirty);
            }
        }
    }
}
