using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Connection/Input")]
    public class InputNode : CustomGraphNode
    {
        [SerializeField] private string _id = "ID";

        protected override void Awake()
        {
            title = "Input";
            mainContainer.AddToClassList("dg-main-container");
            mainContainer.AddToClassList("dg-connection-container");

            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowType));
            outputPort.portName = "";
            outputPort.portColor = FlowType.Color;

            TextField outputTextField = new TextField()
            {
                value = _id,
            };
            outputTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
                HasUnsavedChanges = true;
            });

            outputTextField.AddToClassList("dg-text-field__hidden");
            outputTextField.AddToClassList("dg-text-field__wide");

            outputPort.Add(outputTextField);
            outputContainer.Add(outputPort);
        }
    }
}
