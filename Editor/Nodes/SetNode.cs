using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Celezt.DialogueSystem.Editor
{
    [AssetBinder(typeof(SetInterpreter))]
    [CreateNode("Behaviour/Set", "Set")]
    public class SetNode : DGNode
    {
        [SerializeField]
        private string _propertyName = "None";
        [SerializeField]
        private string _assignOption = "Assign";

        protected override void Awake()
        {
            this.AddStyleSheet(StyleUtility.STYLE_PATH + "Nodes/SetNode");

            Port inputFlowPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(FlowPortType));

            List<string> setOption = new List<string>()
            {
                "Assign",
                "PlusAssign",
                "MinusAssign",
                "MultiplyAssign",
                "DivideAssign",
                "ModAssign",
            };
            List<string> setNames = new List<string>()
            {
                "=",
                "+=",
                "-=",
                "*=",
                "/=",
                "%=",
            };

            DropdownField assignmentField = new DropdownField(setOption, _assignOption, 
            callback =>
            {
                _assignOption = callback;
                hasUnsavedChanges = true;
                return setNames[setOption.IndexOf(callback)];
            });

            inputFlowPort.Add(assignmentField);
            inputContainer.Add(inputFlowPort);

            Port inputSetPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(NumericPortType));

            List<string> names = new List<string>() { "None" };
            names.AddRange(graphView.Blackboard.PropertyNames.Select(x => x.Key));

            DropdownField propertyField = new DropdownField(names, _propertyName, callback =>
            {
                _propertyName = callback;
                hasUnsavedChanges = true;
                return callback;
            });

            graphView.Blackboard.OnPropertyNameChange += callback =>
            {
                bool hasNewName = !string.IsNullOrWhiteSpace(callback.newValue);
                bool hasOldName = !string.IsNullOrWhiteSpace(callback.previousValue);

                if (!hasOldName && hasNewName)  // New property.
                    names.Add(callback.newValue);
                else if (hasOldName && !hasNewName) // Removed property
                {
                    if (propertyField.value == callback.previousValue)  // If current selected is removed.
                    {
                        propertyField.value = names[0];
                        _propertyName = names[0];
                        hasUnsavedChanges = true;
                    }
                    
                    names.Remove(callback.previousValue);
                }
                else if (hasOldName && hasNewName)
                {
                    int index = names.IndexOf(callback.previousValue);
                    if (index != -1)
                        names[index] = callback.newValue;

                    if (propertyField.value == callback.previousValue)  // If current selected changed name.
                    {
                        propertyField.value = callback.newValue;
                        _propertyName = callback.newValue;
                        hasUnsavedChanges = true;
                    }
                }
            };

            inputSetPort.Add(propertyField);
            inputContainer.Add(inputSetPort);

            Port outputFlowPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(FlowPortType));
            outputFlowPort.portName = "Out";
            outputContainer.Add(outputFlowPort);

            RefreshPorts();
        }
    }
}
