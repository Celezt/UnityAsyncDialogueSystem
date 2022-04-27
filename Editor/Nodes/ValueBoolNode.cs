using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [CreateNode("Value/Bool", "")]

    public class ValueBoolNode : DGNode
    {
        [SerializeField] private bool _value = false;

        private enum ConditionState
        {
            False,
            True,
        }
        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/ValueNode");
            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(NumericPortType));
            outputPort.portName = "Bool";
            outputContainer.Add(outputPort);

            var boolState = new EnumField(ConditionState.False)
            {
                value = ToEnum(_value),
            };
            boolState.RegisterValueChangedCallback(x =>
            {
                _value = ToBool((ConditionState)x.newValue);
                hasUnsavedChanges = true;
            });

            static ConditionState ToEnum(bool value) => value switch
            {
                false => ConditionState.False,
                true => ConditionState.True,
            };

            static bool ToBool(ConditionState value) => value switch
            {
                ConditionState.True => true,
                _ => false,
            };

            extensionContainer.Add(boolState);

            RefreshPorts();
            RefreshExpandedState();
        }
    }
}
