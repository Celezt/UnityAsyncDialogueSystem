using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Connection/Output", "Output")]
    public class OutputNode : DGNode
    {
        [SerializeField] private string _id = "ID";

        protected override void Awake()
        {
            mainContainer.AddToClassList("dg-main-container");
            mainContainer.AddToClassList("dg-connection-container");

            //
            // Input Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowType));
            inputPort.portName = "";
            inputPort.portColor = FlowType.Color;

            TextField inputTextField = new TextField()
            {
                value = _id,
            };
            inputTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
                HasUnsavedChanges = true;
            });

            inputTextField.AddToClassList("dg-text-field__hidden");
            inputTextField.AddToClassList("dg-text-field__wide");

            inputPort.Add(inputTextField);
            inputContainer.Add(inputPort);
        }
    }
}
