using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(InputInterpreter))]
    [CreateNode("Connection/Input", "Input")]
    public class InputNode : DGNode
    {
        [SerializeField] private string _id = "default";

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/InputNode");

            //
            // Output Container
            //
            Port outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));

            TextField outputTextField = new TextField()
            {
                name = "Identity",
                value = _id,
            };
            outputTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
                hasUnsavedChanges = true;
            });

            outputPort.Add(outputTextField);
            outputContainer.Add(outputPort);

            RefreshPorts();
        }
    }
}
