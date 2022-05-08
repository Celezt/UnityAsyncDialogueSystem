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
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/ConnectionNode");

            //
            // Input Container
            //
            Port inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowPortType));

            TextField inputTextField = new TextField()
            {
                name = "Identity",
                value = _id,
            };
            inputTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
                hasUnsavedChanges = true;
            });

            inputTextField.AddToClassList("dg-text-field__hidden");
            inputTextField.AddToClassList("dg-text-field__wide");

            inputPort.Add(inputTextField);
            inputContainer.Add(inputPort);

            RefreshPorts();
        }
    }
}
