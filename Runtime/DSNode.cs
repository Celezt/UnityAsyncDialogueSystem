using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSNode
    {
        public IReadOnlyDictionary<string, object> LocalValues => _localValues;
        public IReadOnlyDictionary<int, DSPort> Inputs => _inputs;
        public IReadOnlyDictionary<int, DSPort> Outputs => _outputs;

        public bool IsInterpreter => typeof(AssetInterpreter).IsAssignableFrom(_assetType);
        public bool IsProcessor => typeof(AssetProcessor).IsAssignableFrom(_assetType);

        public bool IsCreated
        {
            get => _created;
            set => _created = value;
        }

        public Type AssetType => _assetType;

        private Dictionary<string, object> _localValues;
        private Dictionary<int, DSPort> _inputs = new Dictionary<int, DSPort>();
        private Dictionary<int, DSPort> _outputs = new Dictionary<int, DSPort>();

        private Type _assetType;
        private bool _created;

        public DSNode(Type assetType, Dictionary<string, object> localValues)
        {
            if (!typeof(IDSAsset).IsAssignableFrom(assetType))
                throw new ArgumentException($"{assetType.FullName} does not inherit {nameof(IDSAsset)}", assetType.FullName);

            _assetType = assetType;
            _localValues = localValues;
        }

        /// <summary>
        /// Insert new port at index. Supports negative index.
        /// </summary>
        /// <param name="index">Positive or negative index.</param>
        /// <param name="direction">Port connection direction.</param>
        /// <returns>Inserted port.</returns>
        public DSPort InsertPort(int index, DSPort.Direction direction)
        {
            DSPort port = new DSPort(this, direction);

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

            return true;
        }

        private bool InternalTryGetAllProcessors(List<AssetProcessor> processors, out AssetProcessor currentProcessor)
        {
            currentProcessor = null;

            if (!IsProcessor)
                return false;

            currentProcessor = (AssetProcessor)ScriptableObject.CreateInstance(_assetType);
            processors.Add(currentProcessor);

            foreach (var (index, input) in _inputs)
            {
                DSNode childNode = input.Connections.FirstOrDefault()?.Output.Node; // Can only have one edge connected to input.

                if (childNode == null)
                    continue;

                if (!childNode.InternalTryGetAllProcessors(processors, out var childProcessor))
                    throw new Exception("Child was not a processor");

                for (int i = currentProcessor._inputs.Count; i <= index; i++)  // Currently does not support vertical input.
                    currentProcessor._inputs.Add(null);

                currentProcessor._inputs.Insert(index, childProcessor);
            }

            return true;
        }
    }
}
