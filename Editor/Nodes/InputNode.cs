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
        protected override void Awake()
        {
            title = "Input";
        }

        protected override void Start()
        {
            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            outputPort.portName = "";

            TextField outputTextField = new TextField()
            {
                value = "ID",
            };
            outputTextField.RegisterValueChangedCallback(callback =>
            {
                
            });

            outputTextField.AddToClassList("dg-text-field__hidden");

            outputPort.Add(outputTextField);
            outputContainer.Add(outputPort);
        }
    }
}
