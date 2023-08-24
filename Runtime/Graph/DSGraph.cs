using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSGraph
    {
        public IReadOnlyDictionary<string, DSNode> InputNodes => _inputNodes;
        public IReadOnlyDictionary<string, List<DSNode>> PropertyNodes => _propertyNodes;
        public IReadOnlyDictionary<string, object> Properties => _properties;

        internal Dictionary<string, DSNode> _inputNodes;
        internal Dictionary<string, List<DSNode>> _propertyNodes;
        internal Dictionary<string, object> _properties;

        public DSGraph()
        {

        }

        public DSNode GetInputNode(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;

            if (!_inputNodes.TryGetValue(id, out var inputID))
                return null;

            return _inputNodes[id];
        }
    }
}
 