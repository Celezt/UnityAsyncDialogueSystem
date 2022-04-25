using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Celezt.DialogueSystem.Editor
{
    public class PropertyNode : DGNode
    {
        protected override void Awake()
        {
           IBlackboardProperty property = userData as IBlackboardProperty;

            outputContainer.Clear();
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, property.PortType);
            outputPort.portName = property.Name;
            outputContainer.Add(outputPort);
        }
    }
}
