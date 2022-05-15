using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(MarkerInterpreter))]
    [CreateNode("Connection/Marker", "Marker")]
    public class MarkerNode : DGNode
    {
        [SerializeField] private string _id = "signal";

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/MarkerNode");

            Port inputFlowPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowPortType));
            inputFlowPort.portName = "In";

            inputContainer.Add(inputFlowPort);

            Port outputFlowPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));
            outputFlowPort.portName = "Out";

            outputContainer.Add(outputFlowPort);

            TextField identifyTextField = new TextField()
            {
                name = "Identity",
                value = _id,
            };
            identifyTextField.RegisterValueChangedCallback(callback =>
            {
                _id = (callback.target as TextField).value;
                hasUnsavedChanges = true;
            });

            controlContainer.Add(identifyTextField);

            RefreshPorts();
        }
    }
}
