using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSNode
    {
        public IReadOnlyDictionary<string, object> LocalValues => _localValues;
        public IReadOnlyDictionary<int, DSPort> Inputs => _inputs;
        public IReadOnlyDictionary<int, DSPort> Outputs => _outputs;

        public Type AssetType => _assetType;

        private Dictionary<string, object> _localValues;
        private Dictionary<int, DSPort> _inputs = new Dictionary<int, DSPort>();
        private Dictionary<int, DSPort> _outputs = new Dictionary<int, DSPort>();

        private Type _assetType;

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
    }
}
