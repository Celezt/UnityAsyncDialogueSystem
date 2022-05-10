using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Celezt.DialogueSystem
{
    public class Dialogue : ScriptableObject
    {
        public DSGraph Graph
        {
            get
            {
                if (_graph == null)
                {
                    _graph = DSUtility.CreateRuntimeGraph(_content);
                }

                return _graph;
            }
        }

        public UnityEvent<DSGraph> OnChanged => _onChanged;

        private DSGraph _graph;

        private UnityEvent<DSGraph> _onChanged = new UnityEvent<DSGraph>();

        [SerializeField, HideInInspector]
        private string _content;

        public Dialogue Initialize(ReadOnlySpan<char> content) 
        {
            if (!MemoryExtensions.Equals(_content, content, StringComparison.Ordinal))
            {
                _content = content.ToString();
                _onChanged.Invoke(Graph);
            }
            else
                _content = content.ToString();

            return this;
        }

        public static implicit operator DSGraph(Dialogue dialogue) => dialogue.Graph;
    }
}
