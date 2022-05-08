using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(ConditionAsset))]
    [CreateNode("Process/Condition", "Condition")]
    public class ConditionNode : DGNode
    {
        [SerializeField]ComparisonType currentComparison = ComparisonType.Equal;

        enum ComparisonType
        {
            Equal,
            NotEqual,
            Less,
            LessOrEqual,
            Greater,
            GreaterOrEqual,
        }

        protected override void Awake()
        {
            //
            // Input Container
            //
            Port inputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(NumericPortType));
            inputPort.portName = "A";
            inputContainer.Add(inputPort);

            Port inputPort2 = this.InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(NumericPortType));
            inputPort2.portName = "B";
            inputContainer.Add(inputPort2);

            //
            // Output Container
            //
            Port outputPort = this.InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(ConditionPortType));
            outputPort.portName = "Out";
            outputContainer.Add(outputPort);

            var enumField = new EnumField(ComparisonType.Equal)
            {
                value = currentComparison,
            };
            enumField.RegisterValueChangedCallback(x =>
            {
                currentComparison = ((ComparisonType)x.newValue);
                hasUnsavedChanges = true;
            });

            controlContainer.Add(enumField);

            RefreshPorts();
        }

        private void Comparison()
        {
            switch (currentComparison)
            {
                case ComparisonType.Greater:
                    return;
                case ComparisonType.GreaterOrEqual:
                    return;
                case ComparisonType.Less:
                    return;
                case ComparisonType.LessOrEqual:
                    return;
                case ComparisonType.NotEqual:
                    return;
                default:           //equal
                    return; 
            }
        } 

    }
}