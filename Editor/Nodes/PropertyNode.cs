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

        private readonly Color _disableColor = new Color(0.3f, 0.3f, 0.3f);

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

                property.RegisterNameChangedCallback(x =>
                {
                    outputPort.portName = x.newValue;
                });

                property.OnDestroyCallback += () =>
                {
                    if (outputPort.connected)
                        graphView.DeleteElements(outputPort.connections);

                    outputPort.portType = typeof(bool);
                    outputPort.portName = $"({property.ValueTypeName} Destroyed)";
                    outputPort.portColor = _disableColor;
                    outputPort.AddToClassList("port-disable");
                    RefreshPorts();
                };
            }
            else
            {
                Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
                outputPort.portName = "(Loading Error)";
                outputPort.portColor = _disableColor;
                outputPort.AddToClassList("port-disable");

                outputContainer.Add(outputPort);
            }

            RefreshPorts();
        }
    }
}
