using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using System;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    public class BasicNode : DGNode
    {
        [SerializeField]
        private string _type;
        [SerializeField]
        private object _value;

        private readonly Color _disableColor = new Color(0.3f, 0.3f, 0.3f);

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/BasicNode");

            if (userData != null)
            {
                Type propertyType = userData as Type;

                if (_type == null)
                    _type = graphView.Blackboard.GetValueType(propertyType).FullName;

                var property = (IBlackboardProperty)Activator.CreateInstance(propertyType);

                if (_value != null)
                    property.Value = Convert.ChangeType(_value, Type.GetType(_type));
                else
                    _value = property.Value;

                property.Initialize(graphView.Blackboard);
                property.RegisterValueChangedCallback(x =>
                {
                    _value = x.newValue;
                });

                outputContainer.Clear();
                Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, graphView.Blackboard.GetPortType(propertyType));
                outputPort.portName = graphView.Blackboard.GetValueName(propertyType);

                VisualElement field = property.BuildController();

                outputContainer.Add(outputPort);
                controlContainer.Add(field);
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
