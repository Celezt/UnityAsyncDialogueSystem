using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Connection/Output")]
    public class OutputNode : CustomGraphNode
    {
        protected override void Awake()
        {
            title = "Output";
        }

        protected override void Start()
        {
            //
            // Output Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "";

            TextField inputTextField = new TextField()
            {
                value = "ID",
            };
            inputTextField.RegisterValueChangedCallback(callback =>
            {

            });

            inputTextField.AddToClassList("dg-text-field__hidden");

            inputPort.Add(inputTextField);
            inputContainer.Add(inputPort);
        }
    }
}
