using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class PropertyNode : DGNode
    {
        public IBlackboardProperty Property
        {
            get => _property;
            set
            {
                _property = value;

                if (_property != null)
                {
                    Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, _property.PortType);
                    outputContainer.Add(outputPort);
                }
            }
        }

        private IBlackboardProperty _property;  
        
        protected override void Awake()
        {
            
        }
    }
}
