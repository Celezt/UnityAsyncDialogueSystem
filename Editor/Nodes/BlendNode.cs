using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(BlendInterpreter))]
    [CreateNode("Process/Blend", "Blend")]
    public class BlendNode : DGNode
    {
        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/BlendNode");

            //
            // Input Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowPortType));
            inputPort.portName = "Base";
            inputContainer.Add(inputPort);

            Port valuePort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(NumericPortType));
            valuePort.portName = "Value";
            inputContainer.Add(valuePort);

            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));
            outputPort.portName = "Blend";
            outputContainer.Add(outputPort);

            RefreshPorts();
        }
    }
}
