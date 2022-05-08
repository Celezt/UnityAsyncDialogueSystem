using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Celezt.DialogueSystem
{
    public class DSGraph
    {
        public IReadOnlyDictionary<string, DSNode> InputNodes => _inputNodes;

        private Dictionary<string, DSNode> _inputNodes;

        public DSGraph(Dictionary<string, DSNode> inputNodes)
        {
            _inputNodes = inputNodes;
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
 