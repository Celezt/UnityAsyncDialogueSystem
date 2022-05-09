using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSNode
    {
        public IReadOnlyDictionary<string, object> Values => _values;
        public IReadOnlyDictionary<int, DSPort> Inputs => _inputs;
        public IReadOnlyDictionary<int, DSPort> Outputs => _outputs;

        public bool IsInterpreter => typeof(AssetInterpreter).IsAssignableFrom(_assetType);
        public bool IsProcessor => typeof(AssetProcessor).IsAssignableFrom(_assetType);
        public bool IsInstanced => _instance != null;

        public Type AssetType => _assetType;

        public object Instance => _instance;

        private Dictionary<string, object> _values;
        private Dictionary<int, DSPort> _inputs = new Dictionary<int, DSPort>();
        private Dictionary<int, DSPort> _outputs = new Dictionary<int, DSPort>();

        private object _instance;
        private Type _assetType;

        public DSNode(Type assetType, Dictionary<string, object> localValues)
        {
            if (!typeof(IDSAsset).IsAssignableFrom(assetType))
                throw new ArgumentException($"{assetType.FullName} does not inherit {nameof(IDSAsset)}", assetType.FullName);

            _assetType = assetType;
            _values = localValues;
        }

        /// <summary>
        /// Insert new port at index. Supports negative index.
        /// </summary>
        /// <param name="index">Positive or negative index.</param>
        /// <param name="direction">Port connection direction.</param>
        /// <returns>Inserted port.</returns>
        public DSPort InsertPort(int index, DSPort.Direction direction)
        {
            DSPort port = new DSPort(this, index, direction);

            if (direction == DSPort.Direction.Input)
                _inputs[index] = port;
            else
                _outputs[index] = port;

            return port;
        }

        /// <summary>
        /// Try get interpreter from node if it exist.
        /// </summary>
        /// <param name="interpreter"><see cref="AssetInterpreter"/> instance.</param>
        /// <returns>If it exist.</returns>
        public bool TryGetInterpreter(out AssetInterpreter interpreter)
        {
            interpreter = null;

            if (!IsInterpreter)
                return false;

            interpreter = (AssetInterpreter)Activator.CreateInstance(_assetType);
            interpreter._node = this;
            _instance = interpreter;

            return true;
        }

        /// <summary>
        /// Try get all processors from node if it exist.
        /// </summary>
        /// <param name="processor"><see cref="AssetProcessor"/> instance.</param>
        /// <returns>If it exist.</returns>
        public bool TryGetAllProcessors(out List<AssetProcessor> processors)
        {
            processors = new List<AssetProcessor>();

            if (InternalTryGetAllProcessors(processors, out var processor))
            {
                processor.InitializeTree(); // Initialize all processors.
            }
            else
                return false;

            return true;
        }

        private bool InternalTryGetAllProcessors(List<AssetProcessor> processors, out AssetProcessor currentProcessor)
        {
            currentProcessor = null;

            bool alreadyExist = false;

            if (!IsProcessor)
                return false;

            if (IsInstanced)
            {
                currentProcessor = (AssetProcessor)_instance;
                alreadyExist = true;
            }
            else
            {
                currentProcessor = (AssetProcessor)ScriptableObject.CreateInstance(_assetType);
                currentProcessor._node = this;
                _instance = currentProcessor;   // Set as instance.
            }

            processors.Add(currentProcessor);

            foreach (var (index, input) in _inputs)
            {
                DSPort childOutput = input.Connections.FirstOrDefault()?.Output;    // Can only have one edge connected to input.

                if (childOutput == null)
                    continue;

                DSNode childNode = childOutput.Node;

                if (!childNode.InternalTryGetAllProcessors(processors, out var childProcessor))
                    continue;

                if (alreadyExist == false)
                {
                    for (int i = currentProcessor._inputs.Count; i <= index; i++)  // Currently does not support vertical input.
                    {
                        currentProcessor._outputPortNumbers.Add(0);
                        currentProcessor._inputs.Add(null);
                    }

                    currentProcessor._inputs.Insert(index, childProcessor);
                    currentProcessor._outputPortNumbers.Insert(index, childOutput.Index);
                }
            }

            return true;
        }
    }
}
