using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Process/Blend", "Blend")]
    public class BlendNode : DGNode
    {
        [SerializeField] private float _timeOffset = 0;

        protected override void Awake()
        {
            //
            // Input Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowType));
            inputPort.portName = "blend";
            inputPort.portColor = FlowType.Color;
            inputContainer.Add(inputPort);

            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowType));
            outputPort.portName = "with";
            outputPort.portColor = FlowType.Color;
            outputContainer.Add(outputPort);

            FloatField blendTextField = new FloatField()
            {
                value = _timeOffset,
            };
            blendTextField.RegisterValueChangedCallback(callback =>
            {
                _timeOffset = (callback.target as FloatField).value;
                HasUnsavedChanges = true;
            });

            extensionContainer.Add(blendTextField);

            RefreshExpandedState();
        }
    }
}
