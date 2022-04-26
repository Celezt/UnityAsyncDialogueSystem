using Celezt.DialogueSystem.Editor.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Value/Float", "")]

    public class ValueFloatNode : DGNode
    {
        [SerializeField] private float _value = 0;
        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/ValueNode");
            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(NumericPortType));
            outputPort.portName = "Float";
            outputContainer.Add(outputPort);

            FloatField valueTextField = new FloatField()
            {
                value = _value,
            };
            valueTextField.RegisterValueChangedCallback(callback =>
            {
                _value = (callback.target as FloatField).value;
                hasUnsavedChanges = true;
            });

            extensionContainer.Add(valueTextField);

            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
