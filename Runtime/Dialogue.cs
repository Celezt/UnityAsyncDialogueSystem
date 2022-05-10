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
                    _graph = DSUtility.CreateDSGraph(_content);
                }

                return _graph;
            }
        }

        private DSGraph _graph;

        [SerializeField, HideInInspector]
        private string _content;

        public Dialogue Initialize(ReadOnlySpan<char> content) 
        {
            _content = content.ToString();

            return this;
        }

        public static implicit operator DSGraph(Dialogue dialogue) => dialogue.Graph;
    }
}
