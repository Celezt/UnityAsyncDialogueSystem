using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Celezt.DialogueSystem.Editor.Utilities;

namespace Celezt.DialogueSystem.Editor
{
    public class PropertyNode : DGNode
    {
        [SerializeField]
        private string _propertyID;

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/PropertyNode");

            if (userData != null)
            {
                IBlackboardProperty property = userData as IBlackboardProperty;

                if (string.IsNullOrEmpty(_propertyID))
                    _propertyID = property.ID.ToString("N");

                outputContainer.Clear();
                Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, property.PortType);
                outputPort.portName = property.Name;
                outputContainer.Add(outputPort);
            }
            else
            {
                Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(BasePortType));
                outputPort.portName = "(Loading Error)";
                outputContainer.Add(outputPort);
            }

            RefreshPorts();
        }
    }
}
